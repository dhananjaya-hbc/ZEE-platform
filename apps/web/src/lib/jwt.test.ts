import { describe, expect, it } from 'vitest';
import { decodeSessionFromToken, isJwtExpired } from './jwt';

function makeToken(payload: Record<string, unknown>): string {
  const header = Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url');
  const body = Buffer.from(JSON.stringify(payload)).toString('base64url');
  return `${header}.${body}.mock-signature`;
}

describe('jwt decoding', () => {
  it('decodes standard RFC 7519 "sub" claim', () => {
    const token = makeToken({
      sub: 'usr-123',
      'zee:university_id': 'uni-456',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    const session = decodeSessionFromToken(token);
    expect(session).toEqual({
      userId: 'usr-123',
      universityId: 'uni-456',
    });
  });

  it('decodes .NET ClaimTypes.NameIdentifier XML URI claim', () => {
    const token = makeToken({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'usr-dotnet-789',
      'zee:university_id': 'uni-dotnet-999',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    const session = decodeSessionFromToken(token);
    expect(session).toEqual({
      userId: 'usr-dotnet-789',
      universityId: 'uni-dotnet-999',
    });
  });

  it('extracts optional name and email claims when present', () => {
    const token = makeToken({
      sub: 'usr-101',
      'zee:university_id': 'uni-202',
      name: 'Ada Lovelace',
      email: 'ada@oxford.ac.uk',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    const session = decodeSessionFromToken(token);
    expect(session).toEqual({
      userId: 'usr-101',
      universityId: 'uni-202',
      name: 'Ada Lovelace',
      email: 'ada@oxford.ac.uk',
    });
  });

  it('returns null if token is expired', () => {
    const expiredToken = makeToken({
      sub: 'usr-123',
      'zee:university_id': 'uni-456',
      exp: Math.floor(Date.now() / 1000) - 100,
    });

    expect(decodeSessionFromToken(expiredToken)).toBeNull();
  });

  it('correctly identifies expired payloads with isJwtExpired', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 1000;
    const pastExp = Math.floor(Date.now() / 1000) - 1000;

    expect(isJwtExpired({ exp: futureExp })).toBe(false);
    expect(isJwtExpired({ exp: pastExp })).toBe(true);
    expect(isJwtExpired({})).toBe(false);
  });

  it('returns null for malformed or missing strings', () => {
    expect(decodeSessionFromToken('')).toBeNull();
    expect(decodeSessionFromToken('invalid-jwt')).toBeNull();
    expect(decodeSessionFromToken('only.two.parts.here?no')).toBeNull();
    expect(decodeSessionFromToken('a.b.c')).toBeNull(); // non-json payload
  });

  it('returns null if missing required claims (sub or universityId)', () => {
    const missingSub = makeToken({
      'zee:university_id': 'uni-456',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });
    const missingUni = makeToken({
      sub: 'usr-123',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    expect(decodeSessionFromToken(missingSub)).toBeNull();
    expect(decodeSessionFromToken(missingUni)).toBeNull();
  });
});
