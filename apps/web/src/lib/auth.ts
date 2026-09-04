/**
 * Client-side authentication state.
 *
 * ZEE has no passwords. Signing in means requesting a one-time code at an
 * institutional address and redeeming it for a JWT:
 *
 *   1. POST /api/auth/request-otp  { email }
 *   2. POST /api/auth/verify-otp   { email, code }  ->  { accessToken }
 *
 * TODO: Implement this module.
 *
 * Acceptance criteria:
 *   - requestOtp(email) and verifyOtp(email, code).
 *   - Expose the current session to components (a context provider is fine).
 *
 * TOKEN STORAGE — decide this deliberately, it is the security-relevant part:
 *   - localStorage is the easy option and is readable by any script that ends up
 *     on the page, so one XSS becomes full account takeover of every signed-in
 *     student.
 *   - An httpOnly, Secure, SameSite=Lax cookie set by a Next route handler is the
 *     safer default and works with the PWA. It needs CSRF protection on mutating
 *     requests, which the cookie approach makes straightforward.
 *   Recommend the cookie route. Whatever is chosen, write down why here.
 */

export interface Session {
  userId: string;
  universityId: string;
  name: string;
  email: string;
}

/** TODO: Implement — POST /api/auth/request-otp. */
export async function requestOtp(email: string): Promise<void> {
  void email;
  throw new Error('Not implemented.');
}

/** TODO: Implement — POST /api/auth/verify-otp, then persist the session. */
export async function verifyOtp(email: string, code: string): Promise<Session> {
  void email;
  void code;
  throw new Error('Not implemented.');
}

/** TODO: Implement — returns the signed-in student, or null. */
export async function getSession(): Promise<Session | null> {
  throw new Error('Not implemented.');
}

/** TODO: Implement — clears the stored token. */
export async function signOut(): Promise<void> {
  throw new Error('Not implemented.');
}
