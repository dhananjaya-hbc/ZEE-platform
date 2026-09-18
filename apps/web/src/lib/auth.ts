/**
 * Client-side authentication state.
 *
 * ZEE has no passwords. Signing in means requesting a one-time code at an
 * institutional address and redeeming it for a JWT:
 *
 *   1. POST /api/auth/request-otp  { email }
 *   2. POST /api/auth/verify-otp   { email, code }  ->  { accessToken, ... }
 *
 * TOKEN STORAGE: an httpOnly cookie, set by app/api/session/route.ts. The token is
 * never held in this module's state and never written to localStorage - it goes
 * straight from the API response into the cookie via one fetch to our own route,
 * and this module never touches it again.
 */

import { request } from '@/lib/api-client';

export interface Session {
  userId: string;
  universityId: string;
  name?: string;
  email?: string;
}

/** Shape of a successful POST /api/auth/verify-otp response. */
interface VerifyOtpResponse {
  accessToken: string;
  userId: string;
  name: string;
  email: string;
  universityId: string;
}

/** Requests a one-time code be emailed to an institutional address. */
export async function requestOtp(email: string): Promise<void> {
  await request<void>('/api/auth/request-otp', {
    method: 'POST',
    body: JSON.stringify({ email }),
  });
}

/**
 * Redeems a one-time code, signing the student in.
 *
 * Note the second fetch call below goes to '/api/session' - a relative, same-origin
 * path to THIS Next.js app's own route handler - not through request() from
 * api-client.ts, which would incorrectly prefix it with the .NET API's base URL.
 */
export async function verifyOtp(email: string, code: string): Promise<Session> {
  const result = await request<VerifyOtpResponse>('/api/auth/verify-otp', {
    method: 'POST',
    body: JSON.stringify({ email, code }),
  });

  await fetch('/api/session', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ accessToken: result.accessToken }),
  });

  return {
    userId: result.userId,
    universityId: result.universityId,
    name: result.name,
    email: result.email,
  };
}

/**
 * Returns the signed-in student, or null.
 *
 * Calls GET /api/session to read the httpOnly cookie server-side and decode the
 * JWT payload.
 */
export async function getSession(): Promise<Session | null> {
  try {
    const res = await fetch('/api/session');
    if (!res.ok) {
      return null;
    }
    return (await res.json()) as Session | null;
  } catch {
    return null;
  }
}

/** Signs the student out by clearing the session cookie. */
export async function signOut(): Promise<void> {
  await fetch('/api/session', { method: 'DELETE' });
}
