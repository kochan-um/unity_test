import { NextResponse } from "next/server";

export type ApiError = {
  code: string;
  message: string;
  details?: unknown;
};

export type ApiMeta = {
  page?: number;
  limit?: number;
  totalCount?: number;
  totalPages?: number;
};

export function ok<T>(data: T, meta?: ApiMeta, init?: ResponseInit) {
  const body = meta ? { data, meta } : { data };
  return NextResponse.json(body, { status: 200, ...init });
}

export function created<T>(data: T, init?: ResponseInit) {
  return NextResponse.json({ data }, { status: 201, ...init });
}

export function fail(status: number, error: ApiError, init?: ResponseInit) {
  return NextResponse.json({ error }, { status, ...init });
}

export function badRequest(message: string, details?: unknown) {
  return fail(400, { code: "bad_request", message, details });
}

export function unauthorized(message = "Unauthorized") {
  return fail(401, { code: "unauthorized", message });
}

export function forbidden(message = "Forbidden") {
  return fail(403, { code: "forbidden", message });
}

export function notFound(message = "Not Found") {
  return fail(404, { code: "not_found", message });
}

export function serverError(message = "Internal Server Error", details?: unknown) {
  return fail(500, { code: "internal_error", message, details });
}
