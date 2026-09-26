// Dependency-free base64url -> UTF-8 decoder (no atob/TextDecoder assumptions).
const B64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';

export interface TokenPayload {
  sub?: string;
  email?: string;
  given_name?: string;
  family_name?: string;
  role?: string;
  tenant?: string;
  [claim: string]: unknown;
}

function base64UrlToBytes(input: string): number[] {
  const padded = input.replace(/-/g, '+').replace(/_/g, '/');
  const bytes: number[] = [];
  let buffer = 0;
  let bits = 0;
  for (const ch of padded) {
    if (ch === '=') break;
    const val = B64.indexOf(ch);
    if (val < 0) continue;
    buffer = (buffer << 6) | val;
    bits += 6;
    if (bits >= 8) {
      bits -= 8;
      bytes.push((buffer >> bits) & 0xff);
    }
  }
  return bytes;
}

function utf8Decode(bytes: number[]): string {
  let out = '';
  let i = 0;
  while (i < bytes.length) {
    const b0 = bytes[i] ?? 0;
    if (b0 < 0x80) {
      out += String.fromCharCode(b0);
      i += 1;
    } else if ((b0 & 0xe0) === 0xc0 && i + 1 < bytes.length) {
      out += String.fromCharCode(((b0 & 0x1f) << 6) | ((bytes[i + 1] ?? 0) & 0x3f));
      i += 2;
    } else if ((b0 & 0xf0) === 0xe0 && i + 2 < bytes.length) {
      out += String.fromCharCode(
        ((b0 & 0x0f) << 12) |
          (((bytes[i + 1] ?? 0) & 0x3f) << 6) |
          ((bytes[i + 2] ?? 0) & 0x3f),
      );
      i += 3;
    } else if ((b0 & 0xf8) === 0xf0 && i + 3 < bytes.length) {
      const cp =
        ((b0 & 0x07) << 18) |
        (((bytes[i + 1] ?? 0) & 0x3f) << 12) |
        (((bytes[i + 2] ?? 0) & 0x3f) << 6) |
        ((bytes[i + 3] ?? 0) & 0x3f);
      const hi = Math.floor((cp - 0x10000) / 0x400) + 0xd800;
      const lo = ((cp - 0x10000) % 0x400) + 0xdc00;
      out += String.fromCharCode(hi, lo);
      i += 4;
    } else {
      i += 1;
    }
  }
  return out;
}

export function decodePayload(token: string): TokenPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3 || !parts[1]) return null;
    const json = utf8Decode(base64UrlToBytes(parts[1]));
    const parsed: unknown = JSON.parse(json);
    if (parsed === null || typeof parsed !== 'object') return null;
    return parsed as TokenPayload;
  } catch {
    return null;
  }
}

const ROLE_CLAIMS = [
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role',
  'role',
];

export function payloadRole(payload: TokenPayload | null): string {
  if (!payload) return '';
  for (const key of ROLE_CLAIMS) {
    const value = payload[key];
    if (typeof value === 'string' && value) return value;
  }
  return '';
}

export function payloadDisplayName(payload: TokenPayload | null): string {
  if (!payload) return 'NALOG';
  const full = `${payload.given_name ?? ''} ${payload.family_name ?? ''}`.trim();
  if (full) return full.toUpperCase();
  if (typeof payload.email === 'string' && payload.email) {
    const fallback = payload.email
      .split('@')[0]
      ?.replace(/[^a-z0-9]+/gi, ' ')
      .trim()
      .toUpperCase();
    if (fallback) return fallback;
  }
  return 'NALOG';
}
