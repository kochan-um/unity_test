import { NextRequest } from "next/server";
import { getSession } from "@/lib/auth/session";
import { unauthorized, notFound } from "@/lib/api/response";

type DemoMessage = {
  id: string;
  role: "user" | "assistant";
  content: string;
  timestamp: string;
};

type DemoSession = {
  id: string;
  admin_user_id: string;
};

const demoMessages: Record<string, DemoMessage[]> = {};
const demoSessions: Record<string, DemoSession> = {};

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

    if (!demoSessions[id]) {
      demoSessions[id] = { id, admin_user_id: session.user.id };
    }

    if (demoSessions[id].admin_user_id !== session.user.id) {
      return notFound("Chat session not found");
    }

    const body = (await request.json()) as { content?: unknown };
    const content = typeof body.content === "string" ? body.content : "";

    if (!demoMessages[id]) {
      demoMessages[id] = [];
    }

    demoMessages[id].push({
      id: `msg-${Date.now()}`,
      role: "user",
      content,
      timestamp: new Date().toISOString(),
    });

    const aiResponse = generateMockResponse(content);

    demoMessages[id].push({
      id: `msg-${Date.now() + 1}`,
      role: "assistant",
      content: aiResponse,
      timestamp: new Date().toISOString(),
    });

    const encoder = new TextEncoder();
    const stream = new ReadableStream({
      start(controller) {
        const chunks = aiResponse.match(/.{1,30}/g) || [];
        let index = 0;

        const sendChunk = () => {
          if (index < chunks.length) {
            const chunk = chunks[index];
            controller.enqueue(
              encoder.encode(
                `data: ${JSON.stringify({ type: "text", content: chunk })}\n\n`
              )
            );
            index += 1;
            setTimeout(sendChunk, 50);
            return;
          }

          controller.enqueue(
            encoder.encode(
              `data: ${JSON.stringify({ type: "done", usage: { promptTokens: 0, completionTokens: 0 } })}\n\n`
            )
          );
          controller.close();
        };

        sendChunk();
      },
    });

    return new Response(stream, {
      headers: {
        "Content-Type": "text/event-stream",
        "Cache-Control": "no-cache",
        Connection: "keep-alive",
      },
    });
  } catch (error: unknown) {
    console.error("Error processing message:", error);
    return new Response(JSON.stringify({ error: "Internal server error" }), {
      status: 500,
    });
  }
}

function generateMockResponse(userMessage: string): string {
  const lowerMessage = userMessage.toLowerCase();

  if (lowerMessage.includes("item") || lowerMessage.includes("weapon")) {
    return "アイテム情報の検索が可能です。カテゴリやレア度を指定して絞り込みできます。";
  }

  if (lowerMessage.includes("player")) {
    return "プレイヤー情報の検索が可能です。プロフィール、レベル、所持品を確認できます。";
  }

  if (lowerMessage.includes("stats") || lowerMessage.includes("dashboard")) {
    return "ダッシュボード統計を取得できます。アクティブ数や総件数を確認してください。";
  }

  if (lowerMessage.includes("hello") || lowerMessage.includes("hi")) {
    return "こんにちは。管理向けチャットアシスタントです。必要な情報を入力してください。";
  }

  return "内容を確認しました。アイテム、プレイヤー、統計などのキーワードで再質問してください。";
}
