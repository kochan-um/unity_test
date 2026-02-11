import { NextRequest } from "next/server";
import { randomUUID } from "crypto";
import { getSession } from "@/lib/auth/session";
import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { sendMessageSchema, formatZodError } from "@/lib/validation/schemas";
import { unauthorized, notFound, badRequest, serverError } from "@/lib/api/response";
import { getGeminiClient, getGeminiModel } from "@/lib/gemini/client";
import { SYSTEM_PROMPT } from "@/lib/gemini/system-prompt";
import { TOOLS } from "@/lib/gemini/tools";
import { executeToolCall } from "@/lib/gemini/tool-handlers";
import type { Database } from "@/lib/supabase/types";

const MAX_REQUESTS_PER_MINUTE = 10;
const DAILY_TOKEN_LIMIT = parseInt(process.env.GEMINI_DAILY_TOKEN_LIMIT || "100000");

// In-memory rate limit store (for 1-minute window)
const rateLimitStore = new Map<
  string,
  { count: number; resetTime: number }
>();

function checkRateLimit(userId: string): { allowed: boolean; retryAfter?: number } {
  const now = Date.now();
  const key = `${userId}:1m`;
  const record = rateLimitStore.get(key);

  if (record && record.resetTime > now) {
    if (record.count >= MAX_REQUESTS_PER_MINUTE) {
      const retryAfter = Math.ceil((record.resetTime - now) / 1000);
      return { allowed: false, retryAfter };
    }
    record.count++;
  } else {
    rateLimitStore.set(key, {
      count: 1,
      resetTime: now + 60 * 1000,
    });
  }

  return { allowed: true };
}

async function checkDailyTokenLimit(
  userId: string,
  tokensToUse: number
): Promise<boolean> {
  const supabase = getSupabaseAdmin();
  const today = new Date().toISOString().split("T")[0];

  const { data } = await supabase
    .from("admin_rate_limits")
    .select("token_count")
    .eq("admin_user_id", userId)
    .eq("date", today)
    .single();

  const currentTokens = data?.token_count || 0;
  if (currentTokens + tokensToUse > DAILY_TOKEN_LIMIT) {
    return false;
  }

  if (data) {
    await supabase
      .from("admin_rate_limits")
      .update({ token_count: currentTokens + tokensToUse })
      .eq("admin_user_id", userId)
      .eq("date", today);
  } else {
    await supabase.from("admin_rate_limits").insert({
      admin_user_id: userId,
      date: today,
      token_count: tokensToUse,
    });
  }

  return true;
}

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    // Check rate limit
    const rateLimitCheck = checkRateLimit(session.user.id);
    if (!rateLimitCheck.allowed) {
      return new Response(null, {
        status: 429,
        statusText: "Too Many Requests",
        headers: {
          "Retry-After": String(rateLimitCheck.retryAfter),
        },
      });
    }

    const supabase = getSupabaseAdmin();

    // Verify session ownership
    const { data: sessionData, error: sessionCheckError } = await supabase
      .from("chat_sessions")
      .select("*")
      .eq("id", id)
      .eq("admin_user_id", session.user.id)
      .single();

    if (sessionCheckError || !sessionData) {
      return notFound("Chat session not found");
    }

    // Parse and validate request
    const body = await request.json();
    const validatedData = sendMessageSchema.parse(body);

    // Save user message
    const userMessageId = randomUUID();
    const now = new Date().toISOString();

    const { error: userMsgError } = await supabase
      .from("chat_messages")
      .insert({
        id: userMessageId,
        session_id: id,
        role: "user",
        content: validatedData.content,
        images: validatedData.images || null,
        created_at: now,
      });

    if (userMsgError) throw userMsgError;

    // Get previous messages for context
    const { data: previousMessages, error: msgError } = await supabase
      .from("chat_messages")
      .select("*")
      .eq("session_id", id)
      .order("created_at", { ascending: true });

    if (msgError) throw msgError;

    // Prepare Gemini request
    const geminiClient = getGeminiClient();
    const model = getGeminiModel();

    // Convert messages to Gemini format
    const conversationHistory = ((previousMessages as Database["public"]["Tables"]["chat_messages"]["Row"][] | null) || []).map((msg) => ({
      role: msg.role === "assistant" ? "model" : "user",
      parts: [{ text: msg.content }],
    }));

    // Add current user message with optional images
    const userParts: any[] = [{ text: validatedData.content }];
    if (validatedData.images && validatedData.images.length > 0) {
      for (const image of validatedData.images) {
        userParts.push({
          inlineData: {
            mimeType: image.mimeType,
            data: image.data,
          },
        });
      }
    }
    conversationHistory.push({
      role: "user",
      parts: userParts,
    });

    // Create streaming response
    const encoder = new TextEncoder();
    let totalTokens = 0;
    let assistantResponse = "";

    const stream = new ReadableStream({
      async start(controller) {
        try {
          const streamResponse = await geminiClient
            .getGenerativeModel({ model, tools: TOOLS, systemInstruction: SYSTEM_PROMPT })
            .generateContentStream({
              contents: conversationHistory,
            });

          // Track tool calls and results
          const toolCalls: any[] = [];
          const toolResults: any[] = [];

          for await (const chunk of streamResponse.stream) {
            const promptTokens = (chunk.usageMetadata as any)?.prompt_token_count || (chunk.usageMetadata as any)?.promptTokens || 0;
            const candidateTokens = (chunk.usageMetadata as any)?.candidate_token_count || (chunk.usageMetadata as any)?.candidateTokens || 0;
            const tokensUsed = promptTokens + candidateTokens;
            totalTokens += tokensUsed;

            // Check daily token limit
            const withinLimit = await checkDailyTokenLimit(
              session.user.id,
              tokensUsed
            );
            if (!withinLimit) {
              controller.enqueue(
                encoder.encode(
                  `data: ${JSON.stringify({ type: "error", message: "Daily token limit exceeded" })}\n\n`
                )
              );
              controller.close();
              return;
            }

            const content = chunk.candidates?.[0]?.content;
            if (!content) continue;

            for (const part of content.parts || []) {
              if (part.text) {
                assistantResponse += part.text;
                controller.enqueue(
                  encoder.encode(
                    `data: ${JSON.stringify({ type: "text", content: part.text })}\n\n`
                  )
                );
              }

              // Handle function calls
              if (part.functionCall) {
                const toolCall = {
                  name: part.functionCall.name,
                  args: part.functionCall.args,
                };
                toolCalls.push(toolCall);

                controller.enqueue(
                  encoder.encode(
                    `data: ${JSON.stringify({ type: "tool_call", name: part.functionCall.name, args: part.functionCall.args })}\n\n`
                  )
                );

                // Execute tool and get result
                const toolResult = await executeToolCall(
                  part.functionCall.name,
                  part.functionCall.args
                );
                toolResults.push({
                  name: part.functionCall.name,
                  result: toolResult.maskedContent,
                });

                controller.enqueue(
                  encoder.encode(
                    `data: ${JSON.stringify({ type: "tool_result", name: part.functionCall.name, content: toolResult.maskedContent })}\n\n`
                  )
                );

                // Add tool result back to conversation for Gemini
                conversationHistory.push({
                  role: "user",
                  parts: [
                    {
                      text: `Tool "${part.functionCall.name}" returned: ${JSON.stringify(toolResult.maskedContent)}`,
                    },
                  ],
                });

                // Continue streaming with tool result context
                const continueStream = await geminiClient
                  .getGenerativeModel({
                    model,
                    tools: TOOLS,
                    systemInstruction: SYSTEM_PROMPT,
                  })
                  .generateContentStream({
                    contents: conversationHistory,
                  });

                for await (const continueChunk of continueStream.stream) {
                  const continueContent = continueChunk.candidates?.[0]?.content;
                  if (!continueContent) continue;

                  for (const continuePart of continueContent.parts || []) {
                    if (continuePart.text) {
                      assistantResponse += continuePart.text;
                      controller.enqueue(
                        encoder.encode(
                          `data: ${JSON.stringify({ type: "text", content: continuePart.text })}\n\n`
                        )
                      );
                    }
                  }
                }
              }
            }
          }

          // Save assistant message
          const { error: assistantMsgError } = await supabase
            .from("chat_messages")
            .insert({
              id: randomUUID(),
              session_id: id,
              role: "assistant",
              content: assistantResponse,
              tool_calls: toolCalls.length > 0 ? toolCalls : null,
              tool_results: toolResults.length > 0 ? toolResults : null,
              created_at: new Date().toISOString(),
            });

          if (assistantMsgError) {
            console.error("Error saving assistant message:", assistantMsgError);
          }

          // Send completion
          controller.enqueue(
            encoder.encode(
              `data: ${JSON.stringify({ type: "done", usage: { promptTokens: totalTokens, completionTokens: 0 } })}\n\n`
            )
          );
          controller.close();
        } catch (error: any) {
          console.error("Error in streaming:", error);
          const errorMessage =
            error instanceof Error ? error.message : "Unknown error";
          controller.enqueue(
            encoder.encode(
              `data: ${JSON.stringify({ type: "error", message: errorMessage })}\n\n`
            )
          );
          controller.close();
        }
      },
    });

    return new Response(stream, {
      headers: {
        "Content-Type": "text/event-stream",
        "Cache-Control": "no-cache",
        Connection: "keep-alive",
      },
    });
  } catch (error: any) {
    if (error.name === "ZodError") {
      return badRequest("Invalid request data", formatZodError(error));
    }
    console.error("Error processing message:", error);
    return serverError();
  }
}
