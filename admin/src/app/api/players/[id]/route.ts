import { getSupabaseAdmin } from "@/lib/supabase/admin";
import type { Database } from "@/lib/supabase/types";
import { badRequest, notFound, ok, serverError } from "@/lib/api/response";
import { formatZodError, updatePlayerSchema } from "@/lib/validation/schemas";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

function resolvePlayerKey(id: string) {
  if (/^\d+$/.test(id)) {
    return { key: "id", value: Number(id) } as const;
  }
  return { key: "user_id", value: id } as const;
}

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const supabase = getSupabaseAdmin();
  const { key, value } = resolvePlayerKey(id);

  const { data, error } = await supabase
    .from("player_profiles")
    .select("*")
    .eq(key, value)
    .maybeSingle();

  if (error) {
    return serverError("Failed to fetch player.", error.message);
  }

  if (!data) {
    return notFound("Player not found.");
  }

  return ok(data);
}

export async function PATCH(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  let payload: unknown = null;
  try {
    payload = await request.json();
  } catch {
    return badRequest("Invalid JSON payload.");
  }

  const parsed = updatePlayerSchema.safeParse(payload);
  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const updates: Database["public"]["Tables"]["player_profiles"]["Update"] = {};
  if (parsed.data.displayName !== undefined) updates.display_name = parsed.data.displayName;
  if (parsed.data.score !== undefined) updates.score = parsed.data.score;
  if (parsed.data.level !== undefined) updates.level = parsed.data.level;
  updates.updated_at = new Date().toISOString();

  const supabase = getSupabaseAdmin();
  const { key, value } = resolvePlayerKey(id);

  const { data, error } = await supabase
    .from("player_profiles")
    .update(updates)
    .eq(key, value)
    .select("*")
    .maybeSingle();

  if (error) {
    return serverError("Failed to update player.", error.message);
  }

  if (!data) {
    return notFound("Player not found.");
  }

  return ok(data);
}




