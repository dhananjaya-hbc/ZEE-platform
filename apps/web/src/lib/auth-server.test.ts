import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getServerSession } from './auth-server';
import { SESSION_COOKIE_NAME } from '@/app/api/session/route';

const mockGet = vi.fn();
vi.mock('next/headers', () => ({
  cookies: () => ({
    get: mockGet,
  }),
}));

function makeToken(payload: Record<string, unknown>): string {
  const header = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
  const body = Buffer.from(JSON.stringify(payload)).toString('base64url');
  return `${header}.${body}.mock-sig`;
}

describe('getServerSession', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('returns null when session cookie is not set', async () => {
    mockGet.mockReturnValue(undefined);

    const session = await getServerSession();
    expect(session).toBeNull();
    expect(mockGet).toHaveBeenCalledWith(SESSION_COOKIE_NAME);
  });

  it('returns session when valid session cookie exists', async () => {
    const token = makeToken({
      sub: 'student-42',
      'zee:university_id': 'uni-1',
      name: 'Alan Turing',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });
    mockGet.mockReturnValue({ value: token });

    const session = await getServerSession();
    expect(session).toEqual({
      userId: 'student-42',
      universityId: 'uni-1',
      name: 'Alan Turing',
    });
  });

  it('returns null when cookie token is expired', async () => {
    const token = makeToken({
      sub: 'student-42',
      'zee:university_id': 'uni-1',
      exp: Math.floor(Date.now() / 1000) - 100,
    });
    mockGet.mockReturnValue({ value: token });

    const session = await getServerSession();
    expect(session).toBeNull();
  });
});
