import { cookies } from "next/headers";

export type SessionUser = {
  id: string;
  email?: string | null;
  role?: string | null;
};

export type SessionData = {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
  user: SessionUser;
};

export const SESSION_COOKIE_NAME = "admin_session";

const encoder = new TextEncoder();
const decoder = new TextDecoder();

function requireSecret(): string {
  const secret = process.env.SESSION_SECRET;
  if (!secret) {
    throw new Error("Missing SESSION_SECRET environment variable.");
  }
  return secret;
}

function base64UrlEncode(bytes: Uint8Array): string {
  if (typeof Buffer !== "undefined") {
    return Buffer.from(bytes)
      .toString("base64")
      .replaceAll("+", "-")
      .replaceAll("/", "_")
      .replaceAll("=", "");
  }

  let binary = "";
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte);
  });

  return btoa(binary).replaceAll("+", "-").replaceAll("/", "_").replaceAll("=", "");
}

function base64UrlDecode(value: string): Uint8Array {
  const base64 = value.replaceAll("-", "+").replaceAll("_", "/");
  const padded = base64 + "===".slice((base64.length + 3) % 4);

  if (typeof Buffer !== "undefined") {
    const buffer = Buffer.from(padded, "base64");
    return new Uint8Array(buffer.buffer, buffer.byteOffset, buffer.byteLength);
  }

  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}

function toArrayBuffer(bytes: Uint8Array): ArrayBuffer {
  return bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength) as ArrayBuffer;
}

async function getKey(secret: string): Promise<CryptoKey> {
  const secretBytes = encoder.encode(secret);
  const hash = await crypto.subtle.digest("SHA-256", secretBytes);
  return crypto.subtle.importKey("raw", hash, "AES-GCM", false, ["encrypt", "decrypt"]);
}

export async function encryptSession(session: SessionData): Promise<string> {
  const secret = requireSecret();
  const key = await getKey(secret);
  const iv = crypto.getRandomValues(new Uint8Array(12));
  const payload = encoder.encode(JSON.stringify(session));
  const ciphertext = new Uint8Array(await crypto.subtle.encrypt({ name: "AES-GCM", iv }, key, payload));

  return `${base64UrlEncode(iv)}.${base64UrlEncode(ciphertext)}`;
}

export async function decryptSession(token: string): Promise<SessionData | null> {
  try {
    const secret = requireSecret();
    const key = await getKey(secret);
    const [ivPart, dataPart] = token.split(".");
    if (!ivPart || !dataPart) {
      return null;
    }

    const iv = base64UrlDecode(ivPart);
    const data = base64UrlDecode(dataPart);
    const plaintext = await crypto.subtle.decrypt(
      { name: "AES-GCM", iv: toArrayBuffer(iv) },
      key,
      toArrayBuffer(data)
    );
    const decoded = decoder.decode(plaintext);
    return JSON.parse(decoded) as SessionData;
  } catch {
    return null;
  }
}

export async function getSessionFromCookieValue(value?: string): Promise<SessionData | null> {
  if (!value) {
    return null;
  }

  const session = await decryptSession(value);
  if (!session) {
    return null;
  }

  if (session.expiresAt && Date.now() > session.expiresAt * 1000) {
    return null;
  }

  return session;
}

export async function getSession(): Promise<SessionData | null> {
  const cookieStore = await cookies();
  const cookieValue = cookieStore.get(SESSION_COOKIE_NAME)?.value;
  return getSessionFromCookieValue(cookieValue);
}

export async function setSessionCookie(session: SessionData): Promise<void> {
  const value = await encryptSession(session);

  const cookieStore = await cookies();
  cookieStore.set(SESSION_COOKIE_NAME, value, {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge: Math.max(0, session.expiresAt - Math.floor(Date.now() / 1000)),
  });
}

export async function clearSessionCookie(): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.set(SESSION_COOKIE_NAME, "", {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
    maxAge: 0,
  });
}
