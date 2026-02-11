import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { badRequest, ok, serverError } from "@/lib/api/response";
import { formatZodError, listPlayersQuerySchema } from "@/lib/validation/schemas";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

function getPaginationRange(page: number, limit: number) {
  const from = (page - 1) * limit;
  const to = from + limit - 1;
  return { from, to };
}

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const parsed = listPlayersQuerySchema.safeParse(Object.fromEntries(searchParams.entries()));

  if (!parsed.success) {
    return badRequest("Validation failed.", formatZodError(parsed.error));
  }

  const { page, limit, search } = parsed.data;
  const { from, to } = getPaginationRange(page, limit);

  const supabase = getSupabaseAdmin();
  let query = supabase.from("player_profiles").select("*", { count: "exact" });

  if (search) {
    query = query.or(`display_name.ilike.%${search}%,user_id.ilike.%${search}%`);
  }

  const { data, error, count } = await query.order("created_at", { ascending: false }).range(from, to);

  if (error) {
    return serverError("Failed to fetch players.", error.message);
  }

  const totalCount = count ?? 0;
  const totalPages = totalCount ? Math.ceil(totalCount / limit) : 0;

  return ok(data ?? [], { page, limit, totalCount, totalPages });
}
