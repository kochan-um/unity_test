import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { forbidden, ok, unauthorized } from "@/lib/api/response";
import { getSession } from "@/lib/auth/session";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET() {
  const session = await getSession();
  if (!session) {
    return unauthorized("Session not found.");
  }

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
}
