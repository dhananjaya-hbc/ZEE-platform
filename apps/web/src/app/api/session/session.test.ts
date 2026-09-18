import { beforeEach, describe, expect, it, vi } from 'vitest';
import { GET, SESSION_COOKIE_NAME } from './route';

const mockGet = vi.fn();
vi.mock('next/headers', () => ({
  cookies: () => ({
    get: mockGet,
    set: vi.fn(),
    delete: vi.fn(),
  }),
}));

function makeToken(payload: Record<string, unknown>): string {
  const header = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
  const body = Buffer.from(JSON.stringify(payload)).toString('base64url');
  return `${header}.${body}.mock-sig`;
}

describe('GET /api/session', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('returns null if no session cookie exists', async () => {
    mockGet.mockReturnValue(undefined);

    const response = await GET();
    expect(response.status).toBe(200);
    const data = await response.json();
    expect(data).toBeNull();
    expect(mockGet).toHaveBeenCalledWith(SESSION_COOKIE_NAME);
  });

  it('returns the session if valid session cookie exists', async () => {
    const token = makeToken({
      sub: 'usr-123',
      'zee:university_id': 'uni-456',
      name: 'Ada Lovelace',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });
    mockGet.mockReturnValue({ value: token });

    const response = await GET();
    expect(response.status).toBe(200);
    const data = await response.json();
    expect(data).toEqual({
      userId: 'usr-123',
      universityId: 'uni-456',
      name: 'Ada Lovelace',
    });
  });

  it('returns null if session cookie holds an expired token', async () => {
    const token = makeToken({
      sub: 'usr-123',
      'zee:university_id': 'uni-456',
      exp: Math.floor(Date.now() / 1000) - 10,
    });
    mockGet.mockReturnValue({ value: token });

    const response = await GET();
    expect(response.status).toBe(200);
    const data = await response.json();
    expect(data).toBeNull();
  });
});
