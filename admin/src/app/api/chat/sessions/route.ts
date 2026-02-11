import { NextRequest } from "next/server";
import { randomUUID } from "crypto";
import { getSession } from "@/lib/auth/session";
import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { createChatSessionSchema } from "@/lib/validation/schemas";
import { created, ok, unauthorized, serverError, badRequest } from "@/lib/api/response";

export async function POST(request: NextRequest) {
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    const body = await request.json();
    const validatedData = createChatSessionSchema.parse(body);

    const supabase = getSupabaseAdmin();
    const sessionId = randomUUID();

    const { error } = await supabase.from("chat_sessions").insert({
      id: sessionId,
      admin_user_id: session.user.id,
      title: validatedData.title || null,
      created_at: new Date().toISOString(),
      updated_at: new Date().toISOString(),
    });

    if (error) throw error;

    return created({
      id: sessionId,
      title: validatedData.title || null,
      created_at: new Date().toISOString(),
    });
  } catch (error: any) {
    if (error.name === "ZodError") {
      return badRequest("Invalid request data", error.issues);
    }
    console.error("Error creating chat session:", error);
    return serverError();
  }
}

export async function GET(request: NextRequest) {
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    const supabase = getSupabaseAdmin();
    const { data, error } = await supabase
      .from("chat_sessions")
      .select("*")
      .eq("admin_user_id", session.user.id)
      .order("created_at", { ascending: false });

    if (error) throw error;

    return ok(data || []);
  } catch (error: any) {
    console.error("Error fetching chat sessions:", error);
    return serverError();
  }
}
