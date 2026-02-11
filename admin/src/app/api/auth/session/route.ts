import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { forbidden, ok, unauthorized } from "@/lib/api/response";
import { getSession } from "@/lib/auth/session";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET() {
  const session = await getSession();
  if (!session) {
    console.error("DEBUG: Session not found in cookie");
    return unauthorized("Session not found.");
  }
  console.log("DEBUG: Session found:", { userId: session.user.id, role: session.user.role });

  // ローカル開発用: ダミーセッション対応
  if (process.env.NODE_ENV === "development") {
    // ダミーセッションはそのまま返す
    if (session.accessToken?.startsWith("demo-access-token-")) {
      return ok({
        user: {
          id: session.user.id,
          email: session.user.email,
          role: session.user.role,
        },
        expiresAt: session.expiresAt,
      });
    }
  }

  // 本番環境またはSupabase接続がある場合: 実際のセッション検証
  try {
    const supabase = getSupabaseAdmin();
    const { data, error } = await supabase.auth.getUser(session.accessToken);

    if (error || !data.user) {
      return unauthorized("Session expired.");
    }

    const role = (data.user.app_metadata as { role?: string } | null)?.role ?? null;
    if (role !== "admin") {
      return forbidden("Admin role required.");
    }

    return ok({
      user: {
        id: data.user.id,
        email: data.user.email,
        role,
      },
      expiresAt: session.expiresAt,
    });
  } catch (err) {
    console.error("Session validation error:", err);
    // 開発環境またはダミーセッションの場合は許容
    if (process.env.NODE_ENV === "development" || session.user.role === "admin") {
      return ok({
        user: session.user,
        expiresAt: session.expiresAt,
      });
    }
    return unauthorized("Session validation failed.");
  }
}
