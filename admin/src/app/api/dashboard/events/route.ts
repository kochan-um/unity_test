import { getSupabaseAdmin } from "@/lib/supabase/admin";
import { unauthorized } from "@/lib/api/response";
import { getSession } from "@/lib/auth/session";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

export async function GET(request: Request) {
  const session = await getSession();
  if (!session) {
    return unauthorized("Session not found.");
  }

  const supabase = getSupabaseAdmin();
  const encoder = new TextEncoder();

  const stream = new ReadableStream({
    start(controller) {
      const send = (event: string, payload: unknown) => {
        controller.enqueue(encoder.encode(`event: ${event}\n`));
        controller.enqueue(encoder.encode(`data: ${JSON.stringify(payload)}\n\n`));
      };

      const channel = supabase.channel("admin-events");

      channel.on(
        "postgres_changes",
        { event: "*", schema: "public", table: "player_profiles" },
        (payload) => send("player_profiles", payload)
      );

      channel.on(
        "postgres_changes",
        { event: "*", schema: "public", table: "items" },
        (payload) => send("items", payload)
      );

      channel.on(
        "postgres_changes",
        { event: "*", schema: "public", table: "inventory_items" },
        (payload) => send("inventory_items", payload)
      );

      channel.subscribe((status) => {
        if (status === "SUBSCRIBED") {
          send("ready", { status: "subscribed" });
        }
      });

      const keepAlive = setInterval(() => {
        controller.enqueue(encoder.encode(`: keep-alive\n\n`));
      }, 15000);

      const cleanup = async () => {
        clearInterval(keepAlive);
        await supabase.removeChannel(channel);
        controller.close();
      };

      request.signal.addEventListener("abort", () => {
        void cleanup();
      });
    },
  });

  return new Response(stream, {
    headers: {
      "Content-Type": "text/event-stream",
      "Cache-Control": "no-cache, no-transform",
      Connection: "keep-alive",
    },
  });
}
