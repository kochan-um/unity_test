import { getSupabaseAdmin } from "@/lib/supabase/admin";
import type { Database } from "@/lib/supabase/types";
import { badRequest, notFound, ok, serverError } from "@/lib/api/response";
import { formatZodError, updateItemSchema } from "@/lib/validation/schemas";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase
    .from("items")
    .select("*")
    .eq("id", id)
    .eq("is_deleted", false)
    .maybeSingle();

  if (error) {
    return serverError("Failed to fetch item.", error.message);
  }

  if (!data) {
    return notFound("Item not found.");
  }

  return ok(data);
}

export async function PUT(
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

  const parsed = updateItemSchema.safeParse(payload);
  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const updates: Database["public"]["Tables"]["items"]["Update"] = {};

  if (parsed.data.name !== undefined) updates.name = parsed.data.name;
  if (parsed.data.description !== undefined) updates.description = parsed.data.description;
  if (parsed.data.category !== undefined) updates.category = parsed.data.category;
  if (parsed.data.rarity !== undefined) updates.rarity = parsed.data.rarity;
  if (parsed.data.iconUrl !== undefined) updates.icon_url = parsed.data.iconUrl;
  if (parsed.data.stackable !== undefined) updates.stackable = parsed.data.stackable;
  if (parsed.data.maxStack !== undefined) updates.max_stack = parsed.data.maxStack;
  updates.updated_at = new Date().toISOString();

  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase
    .from("items")
    .update(updates)
    .eq("id", id)
    .eq("is_deleted", false)
    .select("*")
    .maybeSingle();

  if (error) {
    return serverError("Failed to update item.", error.message);
  }

  if (!data) {
    return notFound("Item not found.");
  }

  return ok(data);
}

export async function DELETE(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const supabase = getSupabaseAdmin();
  const { data, error } = await supabase
    .from("items")
    .update({ is_deleted: true, updated_at: new Date().toISOString() })
    .eq("id", id)
    .eq("is_deleted", false)
    .select("*")
    .maybeSingle();

  if (error) {
    return serverError("Failed to delete item.", error.message);
  }

  if (!data) {
    return notFound("Item not found.");
  }

  return ok(data);
}




