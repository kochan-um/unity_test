import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { badRequest, forbidden, ok, unauthorized } from "@/lib/api/response";
import { loginSchema, formatZodError } from "@/lib/validation/schemas";
import { setSessionCookie } from "@/lib/auth/session";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

// ローカル開発用ダミー認証
const DEMO_CREDENTIALS = {
  email: "admin@example.com",
  password: "password123",
};

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

  // ローカル開発用: ダミー認証を使用
  if (
    process.env.NODE_ENV === "development" &&
    parsed.data.email === DEMO_CREDENTIALS.email &&
    parsed.data.password === DEMO_CREDENTIALS.password
  ) {
    const demoUserId = "demo-user-" + Date.now();
    console.log("DEBUG: Demo login attempt - setting session cookie");
    await setSessionCookie({
      accessToken: "demo-access-token-" + demoUserId,
      refreshToken: "demo-refresh-token-" + demoUserId,
      expiresAt: Math.floor(Date.now() / 1000) + 86400, // 24時間有効
      user: {
        id: demoUserId,
        email: parsed.data.email,
        role: "admin",
      },
    });

    console.log("DEBUG: Demo session cookie set successfully");
    return ok({
      user: {
        id: demoUserId,
        email: parsed.data.email,
        role: "admin",
      },
    });
  }

  // 本番環境またはSupabase接続がある場合: 実際の認証
  try {
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
  } catch (err) {
    console.error("Auth error:", err);
    return unauthorized("Authentication failed.");
  }
}
