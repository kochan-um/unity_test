import { NextRequest } from "next/server";
import { getSession } from "@/lib/auth/session";
import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { ok, unauthorized, notFound, serverError } from "@/lib/api/response";

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    const supabase = getSupabaseAdmin();

    // Verify session ownership
    const { data: sessionData, error: sessionError } = await supabase
      .from("chat_sessions")
      .select("*")
      .eq("id", id)
      .eq("admin_user_id", session.user.id)
      .single();

    if (sessionError || !sessionData) {
      return notFound("Chat session not found");
    }

    // Get messages
    const { data: messages, error: messagesError } = await supabase
      .from("chat_messages")
      .select("*")
      .eq("session_id", id)
      .order("created_at", { ascending: true });

    if (messagesError) throw messagesError;

    return ok({
      ...sessionData,
      messages: messages || [],
    });
  } catch (error: unknown) {
    console.error("Error fetching chat session:", error);
    return serverError();
  }
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  try {
    const session = await getSession();
    if (!session) {
      return unauthorized();
    }

    const supabase = getSupabaseAdmin();

    // Verify session ownership
    const { data: sessionData, error: sessionCheckError } = await supabase
      .from("chat_sessions")
      .select("id")
      .eq("id", id)
      .eq("admin_user_id", session.user.id)
      .single();

    if (sessionCheckError || !sessionData) {
      return notFound("Chat session not found");
    }

    // Delete messages first
    const { error: messagesError } = await supabase
      .from("chat_messages")
      .delete()
      .eq("session_id", id);

    if (messagesError) throw messagesError;

    // Delete session
    const { error: sessionError } = await supabase
      .from("chat_sessions")
      .delete()
      .eq("id", id);

    if (sessionError) throw sessionError;

    return ok({ success: true });
  } catch (error: unknown) {
    console.error("Error deleting chat session:", error);
    return serverError();
  }
}

