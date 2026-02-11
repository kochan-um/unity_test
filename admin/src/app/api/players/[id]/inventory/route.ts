import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { notFound, ok, serverError } from "@/lib/api/response";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

async function resolveUserId(id: string) {
  if (!/^\d+$/.test(id)) {
    return id;
  }

  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase
    .from("player_profiles")
    .select("user_id")
    .eq("id", Number(id))
    .maybeSingle();

  if (error) {
    throw new Error(error.message);
  }

  return data?.user_id ?? null;
}

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  let userId: string | null = null;

  try {
    userId = await resolveUserId(id);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unexpected error";
    return serverError("Failed to fetch player inventory.", message);
  }

  if (!userId) {
    return notFound("Player not found.");
  }

  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase
    .from("inventory_items")
    .select("*")
    .eq("user_id", userId)
    .order("slot_index", { ascending: true });

  if (error) {
    return serverError("Failed to fetch inventory.", error.message);
  }

  return ok(data ?? []);
}
