import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { badRequest, forbidden, ok, unauthorized } from "@/lib/api/response";
import { loginSchema, formatZodError } from "@/lib/validation/schemas";
import { setSessionCookie } from "@/lib/auth/session";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function POST(request: Request) {
  let payload: unknown = null;
  try {
    payload = await request.json();
  } catch {
    return badRequest("Invalid JSON payload.");
  }

  const parsed = loginSchema.safeParse(payload);
  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase.auth.signInWithPassword({
    email: parsed.data.email,
    password: parsed.data.password,
  });

  if (error || !data.session || !data.user) {
    return unauthorized("Invalid credentials.");
  }

  const role = (data.user.app_metadata as { role?: string } | null)?.role ?? null;
  if (role !== "admin") {
    return forbidden("Admin role required.");
  }

  await setSessionCookie({
    accessToken: data.session.access_token,
    refreshToken: data.session.refresh_token,
    expiresAt: data.session.expires_at ?? Math.floor(Date.now() / 1000) + 3600,
    user: {
      id: data.user.id,
      email: data.user.email,
      role,
    },
  });

  return ok({
    user: {
      id: data.user.id,
      email: data.user.email,
      role,
    },
  });
}
