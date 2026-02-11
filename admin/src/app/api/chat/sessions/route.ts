import { NextRequest } from "next/server";
import { randomUUID } from "crypto";
import { getSession } from "@/lib/auth/session";
import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { createChatSessionSchema } from "@/lib/validation/schemas";
import { created, ok, unauthorized, serverError, badRequest } from "@/lib/api/response";

// ローカル開発用ダミーデータ
const demoSessions: { [key: string]: any } = {};

export async function POST(request: NextRequest) {
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    const body = await request.json();
    const validatedData = createChatSessionSchema.parse(body);
    const sessionId = randomUUID();
    const now = new Date().toISOString();

    // ローカル開発用: ダミーセッション保存
    if (process.env.NODE_ENV === "development" && session.user.role === "admin") {
      demoSessions[sessionId] = {
        id: sessionId,
        admin_user_id: session.user.id,
        title: validatedData.title || null,
        created_at: now,
        updated_at: now,
      };

      return created({
        data: {
          id: sessionId,
          title: validatedData.title || null,
          created_at: now,
        },
      });
    }

    // 本番環境: 実際のSupabase操作
    const supabase = getSupabaseAdmin();
    const { error } = await supabase.from("chat_sessions").insert({
      id: sessionId,
      admin_user_id: session.user.id,
      title: validatedData.title || null,
      created_at: now,
      updated_at: now,
    });

    if (error) throw error;

    return created({
      data: {
        id: sessionId,
        title: validatedData.title || null,
        created_at: now,
      },
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

    // ローカル開発用: ダミーセッション取得
    if (process.env.NODE_ENV === "development" && session.user.role === "admin") {
      const userSessions = Object.values(demoSessions).filter(
        (s) => s.admin_user_id === session.user.id
      );
      return ok(userSessions);
    }

    // 本番環境: 実際のSupabase操作
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
