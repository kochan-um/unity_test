import { NextResponse, type NextRequest } from "next/server";

import { getSessionFromCookieValue, SESSION_COOKIE_NAME } from "@/lib/auth/session";

const PUBLIC_PATHS = new Set(["/api/auth/login", "/api/health"]);

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (!pathname.startsWith("/api/")) {
    return NextResponse.next();
  }

  if (PUBLIC_PATHS.has(pathname)) {
    return NextResponse.next();
  }

  if (request.method === "OPTIONS") {
    return NextResponse.next();
  }

  const cookieValue = request.cookies.get(SESSION_COOKIE_NAME)?.value;
  const session = await getSessionFromCookieValue(cookieValue);

  if (!session) {
    return NextResponse.json(
      { error: { code: "unauthorized", message: "Unauthorized" } },
      { status: 401 }
    );
  }

  if (session.user?.role !== "admin") {
    return NextResponse.json(
      { error: { code: "forbidden", message: "Forbidden" } },
      { status: 403 }
    );
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/api/:path*"],
};
