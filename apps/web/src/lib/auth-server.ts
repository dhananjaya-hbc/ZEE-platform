import { cookies } from 'next/headers';
import { SESSION_COOKIE_NAME } from '@/app/api/session/route';
import { decodeSessionFromToken } from './jwt';
import type { Session } from './auth';

/**
 * Reads and decodes the current session on the server side (Server Components,
 * Server Actions, or Route Handlers).
 *
 * Reads the httpOnly session cookie directly via Next.js cookies() without
 * making an unnecessary network hop.
 *
 * Returns null if no session cookie exists, or if the token is expired or malformed.
 */
export async function getServerSession(): Promise<Session | null> {
  const cookieStore = await cookies();
  const sessionCookie = cookieStore.get(SESSION_COOKIE_NAME);

  if (!sessionCookie?.value) {
    return null;
  }

  return decodeSessionFromToken(sessionCookie.value);
}
