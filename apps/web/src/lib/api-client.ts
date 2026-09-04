/**
 * Typed client for the ZEE API.
 *
 * All backend access goes through here rather than scattering `fetch` calls across
 * components — one place to attach auth, one place to translate errors, one place
 * to change when the base URL moves.
 *
 * NOTE: the browser never talks to the AI service directly. Chatbot calls go to
 * this API, which proxies them with an internal key held server-side.
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
 * Performs a request against the API.
 *
 * TODO: Implement.
 *
 * Acceptance criteria:
 *   - Prefix `path` with API_BASE_URL; send/accept JSON.
 *   - Attach the bearer token when the caller is signed in. Read it from wherever
 *     `lib/auth.ts` ends up storing it — see the TODO there. Do NOT put the token
 *     in localStorage without reading that note first.
 *   - On a non-2xx response, parse the body as ProblemDetails and throw ApiError.
 *     Handle a non-JSON body (a proxy 502, say) without throwing a parse error over
 *     the top of the real failure.
 *   - Accept an AbortSignal so callers can cancel in-flight requests on unmount.
 */
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  void path;
  void init;
  void API_BASE_URL;
  throw new Error('Not implemented: see the TODO on request().');
}

export const api = {
  /**
   * Fetches one page of the feed, newest first.
   *
   * TODO: Implement via request(). Pass `cursor` straight through as a query
   * parameter; never build one client-side.
   */
  getFeed(cursor?: string, limit = 20): Promise<CursorPage<PostDto>> {
    void cursor;
    void limit;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — POST /api/posts. */
  createPost(content: string): Promise<PostDto> {
    void content;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — POST /api/chatbot/ask. Returns mock answers in Phase 1. */
  askChatbot(question: string): Promise<ChatbotAnswer> {
    void question;
    throw new Error('Not implemented.');
  },

  /** TODO: Implement — GET /api/events. */
  getUpcomingEvents(): Promise<EventDto[]> {
    throw new Error('Not implemented.');
  },
};

export { request };
