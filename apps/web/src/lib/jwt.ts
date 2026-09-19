import type { Session } from './auth';

export interface JwtPayload {
  sub?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'zee:university_id'?: string;
  universityId?: string;
  email?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  name?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  exp?: number;
  nbf?: number;
  iat?: number;
  iss?: string;
  aud?: string;
  [key: string]: unknown;
}

/**
 * Decodes a base64url string across Node.js, browser, and edge runtimes.
 */
export function base64UrlDecode(input: string): string {
  let base64 = input.replace(/-/g, '+').replace(/_/g, '/');
  while (base64.length % 4 !== 0) {
    base64 += '=';
  }

  if (typeof Buffer !== 'undefined') {
    return Buffer.from(base64, 'base64').toString('utf-8');
  }

  if (typeof atob !== 'undefined') {
    const raw = atob(base64);
    const bytes = Uint8Array.from(raw, (c) => c.charCodeAt(0));
    return new TextDecoder().decode(bytes);
  }

  throw new Error('No base64 decoder available in current environment.');
}

/**
 * Checks whether a JWT payload has expired according to its `exp` claim.
 */
export function isJwtExpired(payload: JwtPayload): boolean {
  if (typeof payload.exp !== 'number') {
    return false;
  }
  // exp is in seconds, Date.now() is in milliseconds
  return Date.now() >= payload.exp * 1000;
}

/**
 * Decodes and validates a raw JWT string into a typed Session.
 *
 * Supports both standard RFC 7519 claims ("sub") and .NET ClaimTypes URI claims
 * ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").
 *
 * Returns null if the token is malformed, missing required claims, or expired.
 */
export function decodeSessionFromToken(token: string): Session | null {
  if (!token || typeof token !== 'string') {
    return null;
  }

  const parts = token.split('.');
  if (parts.length !== 3) {
    return null;
  }

  try {
    const payloadPart = parts[1];
    if (!payloadPart) {
      return null;
    }
    const json = base64UrlDecode(payloadPart);
    const payload = JSON.parse(json) as JwtPayload;

    if (isJwtExpired(payload)) {
      return null;
    }

    const userId =
      payload.sub ||
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    const universityId = payload['zee:university_id'] || payload.universityId;

    if (
      !userId ||
      !universityId ||
      typeof userId !== 'string' ||
      typeof universityId !== 'string'
    ) {
      return null;
    }

    const name =
      (typeof payload.name === 'string' ? payload.name : undefined) ||
      (typeof payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] === 'string'
        ? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
        : undefined);

    const email =
      (typeof payload.email === 'string' ? payload.email : undefined) ||
      (typeof payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] === 'string'
        ? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']
        : undefined);

    return {
      userId,
      universityId,
      ...(name ? { name } : {}),
      ...(email ? { email } : {}),
    };
  } catch {
    return null;
  }
}
