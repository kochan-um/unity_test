import { NextRequest } from "next/server";
import { randomUUID } from "crypto";
import { ZodError } from "zod";
import { getSession } from "@/lib/auth/session";
import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { createChatSessionSchema } from "@/lib/validation/schemas";
import { created, ok, unauthorized, serverError, badRequest } from "@/lib/api/response";

type DemoChatSession = {
  id: string;
  admin_user_id: string;
  title: string | null;
  created_at: string;
  updated_at: string;
};

const demoSessions: Record<string, DemoChatSession> = {};

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
  } catch (error: unknown) {
    if (error instanceof ZodError) {
      return badRequest("Invalid request data", error.issues);
    }

    console.error("Error creating chat session:", error);
    return serverError();
  }
}

export async function GET(_request: NextRequest) {
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    if (process.env.NODE_ENV === "development" && session.user.role === "admin") {
      const userSessions = Object.values(demoSessions).filter(
        (chatSession) => chatSession.admin_user_id === session.user.id
      );
      return ok(userSessions);
    }

    const supabase = getSupabaseAdmin();
    const { data, error } = await supabase
      .from("chat_sessions")
      .select("*")
      .eq("admin_user_id", session.user.id)
      .order("created_at", { ascending: false });

    if (error) throw error;

    return ok(data || []);
  } catch (error: unknown) {
    console.error("Error fetching chat sessions:", error);
    return serverError();
  }
}
