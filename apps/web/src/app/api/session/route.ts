import { NextResponse } from 'next/server';
import { cookies } from 'next/headers';
import { decodeSessionFromToken } from '@/lib/jwt';

/**
 * Session cookie used across the app. Exported so any future code that needs to
 * read it (a getSession() implementation, middleware route-guarding) uses this same
 * name rather than a second hard-coded string that can drift out of sync.
 */
export const SESSION_COOKIE_NAME = 'zee_session';

/**
 * Matches JwtOptions.AccessTokenLifetimeMinutes' default on the API (60 minutes).
 * If that value changes, update this too - a mismatch is not dangerous (an expired
 * JWT still gets rejected by the API regardless of how long the cookie itself lives),
 * just confusing: the UI would look "signed in" for longer than the token is actually
 * valid.
 */
const MAX_AGE_SECONDS = 60 * 60;

/**
 * Stores a freshly issued access token as an httpOnly cookie.
 *
 * Called once, right after the browser calls the .NET API's /api/auth/verify-otp
 * directly and receives a token back. This route exists so that token never has to
 * be written to localStorage or held in client-side state - it goes straight from an
 * in-memory JS variable into a cookie JavaScript can never read again.
 */
export async function POST(request: Request) {
  const body = (await request.json()) as { accessToken?: string };

  if (!body.accessToken) {
    return NextResponse.json({ error: 'accessToken is required' }, { status: 400 });
  }

  const cookieStore = await cookies();

  cookieStore.set(SESSION_COOKIE_NAME, body.accessToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: MAX_AGE_SECONDS,
  });

  return new NextResponse(null, { status: 204 });
}

/**
 * Returns the signed-in student session, or null if unauthenticated or expired.
 *
 * Reads the httpOnly cookie server-side and decodes the JWT payload.
 */
export async function GET() {
  const cookieStore = await cookies();
  const sessionCookie = cookieStore.get(SESSION_COOKIE_NAME);

  if (!sessionCookie?.value) {
    return NextResponse.json(null);
  }

  const session = decodeSessionFromToken(sessionCookie.value);
  return NextResponse.json(session);
}

/** Signs the student out by clearing the session cookie. */
export async function DELETE() {
  const cookieStore = await cookies();
  cookieStore.delete(SESSION_COOKIE_NAME);

  return new NextResponse(null, { status: 204 });
}

