import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { badRequest, created, ok, serverError } from "@/lib/api/response";
import { createItemSchema, formatZodError, listItemsQuerySchema } from "@/lib/validation/schemas";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

function getPaginationRange(page: number, limit: number) {
  const from = (page - 1) * limit;
  const to = from + limit - 1;
  return { from, to };
}

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const parsed = listItemsQuerySchema.safeParse(Object.fromEntries(searchParams.entries()));

  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const { page, limit, category, rarity, search, sort, order } = parsed.data;
  const { from, to } = getPaginationRange(page, limit);

  const supabase = getSupabaseAdmin();
  let query = supabase
    .from("items")
    .select("*", { count: "exact" })
    .eq("is_deleted", false);

  if (category) {
    query = query.eq("category", category);
  }

  if (rarity) {
    query = query.eq("rarity", rarity);
  }

  if (search) {
    query = query.ilike("name", `%${search}%`);
  }

  query = query.order(sort ?? "created_at", { ascending: (order ?? "desc") === "asc" });

  const { data, error, count } = await query.range(from, to);

  if (error) {
    return serverError("Failed to fetch items.", error.message);
  }

  const totalCount = count ?? 0;
  const totalPages = totalCount ? Math.ceil(totalCount / limit) : 0;

  return ok(data ?? [], { page, limit, totalCount, totalPages });
}

export async function POST(request: Request) {
  let payload: unknown = null;
  try {
    payload = await request.json();
  } catch {
    return badRequest("Invalid JSON payload.");
  }

  const parsed = createItemSchema.safeParse(payload);
  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const supabase = getSupabaseAdmin();
  const maxStack = parsed.data.stackable ? parsed.data.maxStack ?? 1 : 1;

  const { data, error } = await supabase
    .from("items")
    .insert({
      name: parsed.data.name,
      description: parsed.data.description ?? null,
      category: parsed.data.category ?? null,
      rarity: parsed.data.rarity ?? null,
      stackable: parsed.data.stackable,
      max_stack: maxStack,
      icon_url: parsed.data.iconUrl ?? null,
      is_deleted: false,
    })
    .select("*")
    .single();

  if (error) {
    return serverError("Failed to create item.", error.message);
  }

  return created(data);
}
