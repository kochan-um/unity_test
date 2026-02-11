import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { ok, serverError } from "@/lib/api/response";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET() {
  const supabase = getSupabaseAdmin();
  const todayStart = new Date();
  todayStart.setUTCHours(0, 0, 0, 0);

  try {
    const [players, activePlayers, items, inventory] = await Promise.all([
      supabase.from("player_profiles").select("id", { count: "exact", head: true }),
      supabase
        .from("player_profiles")
        .select("id", { count: "exact", head: true })
        .gte("updated_at", todayStart.toISOString()),
      supabase.from("items").select("id", { count: "exact", head: true }).eq("is_deleted", false),
      supabase.from("inventory_items").select("user_id", { count: "exact", head: true }),
    ]);

    const errors = [players.error, activePlayers.error, items.error, inventory.error].filter(Boolean);
    if (errors.length > 0) {
      return serverError("Failed to load dashboard stats.", errors.map((err) => err?.message));
    }

    return ok({
      totalPlayers: players.count ?? 0,
      activeToday: activePlayers.count ?? 0,
      totalItems: items.count ?? 0,
      totalInventoryRows: inventory.count ?? 0,
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unexpected error";
    return serverError("Failed to load dashboard stats.", message);
  }
}
