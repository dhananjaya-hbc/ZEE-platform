/**
 * Typed client for the ZEE API.
 *
 * All backend access goes through here rather than scattering `fetch` calls across
 * components — one place to attach auth, one place to translate errors, one place
 * to change when the base URL moves.
 *
 * NOTE: the browser never talks to the AI service directly. Chatbot calls go to
 * this API, which proxies them with an internal key held server-side.
 *
 * NOTE ON AUTH: request() does not attach a bearer token. The session cookie set by
 * app/api/session/route.ts is httpOnly (client JS cannot read it) and scoped to this
 * Next.js app's own origin, not the .NET API's — so it is never available here to
 * attach as an Authorization header. Endpoints that need one (anything but the two
 * auth endpoints below) will need to go through a Next.js server-side route that
 * reads the cookie itself and forwards the request with the header attached - not
 * built yet, since nothing calling from here needs it today.
 */

import type {
  ChatbotAnswer,
  CursorPage,
  EventDto,
  PostDto,
  ProblemDetails,
} from '@/types/api';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5080';

/** Error thrown for any non-2xx response, carrying the parsed problem document. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly problem: ProblemDetails,
  ) {
    super(problem.title ?? `Request failed with status ${status}`);
    this.name = 'ApiError';
  }

  /**
   * True when the endpoint is scaffolded but not yet implemented.
   *
   * The API returns 501 for any handler still throwing NotImplementedException,
   * which lets the UI show "coming soon" instead of a generic failure while Phase 1
   * is being built out.
   */
  get isNotImplemented(): boolean {
    return this.status === 501;
  }

  /** Field-keyed validation messages, ready to attach to form inputs. */
  get fieldErrors(): Record<string, string[]> {
    return this.problem.errors ?? {};
  }
}

/**
 * Parses a failed response's body as a problem document.
 *
 * Falls back to a minimal document built from the status line when the body is not
 * JSON at all - a proxy timeout returning an HTML error page, for instance - so that
 * case surfaces as the real HTTP failure rather than a JSON parse error masking it.
 */
async function parseProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return { status: response.status, title: response.statusText };
  }
}

/**
 * Performs a request against the API.
 *
 * `init.signal` (a standard fetch AbortSignal) passes straight through, so callers
 * can cancel an in-flight request on unmount by including one.
 */
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw new ApiError(response.status, await parseProblem(response));
  }

  // 204 No Content (e.g. request-otp's response) has no body to parse.
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const api = {
  /**
   * Fetches one page of the feed, newest first.
   *
   * TODO: Implement via request(). Pass `cursor` straight through as a query
   * parameter; never build one client-side. Needs the auth proxy described above
   * before it can work, since /api/feed requires a signed-in student.
   */
  getFeed(cursor?: string, limit = 20): Promise<CursorPage<PostDto>> {
    void cursor;
    void limit;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — POST /api/posts. Needs the auth proxy, same as getFeed. */
  createPost(content: string): Promise<PostDto> {
    void content;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — POST /api/chatbot/ask. Needs the auth proxy, same as getFeed. */
  askChatbot(question: string): Promise<ChatbotAnswer> {
    void question;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — GET /api/events. Needs the auth proxy, same as getFeed. */
  getUpcomingEvents(): Promise<EventDto[]> {
    throw new Error('Not implemented.');
  },
};

export { request };
