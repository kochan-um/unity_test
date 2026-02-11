import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { fail, ok } from "@/lib/api/response";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET() {
  try {
    const supabase = getSupabaseAdmin();
    const { error } = await supabase.from("player_profiles").select("id", { count: "exact", head: true });

    if (error) {
      return fail(503, { code: "unhealthy", message: "Supabase connection failed.", details: error.message });
    }

    return ok({ status: "ok" });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unexpected error";
    return fail(503, { code: "unhealthy", message });
  }
}
