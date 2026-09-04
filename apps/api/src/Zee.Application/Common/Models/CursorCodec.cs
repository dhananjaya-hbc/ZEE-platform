using Zee.Domain.Common;

namespace Zee.Application.Common.Models;

/// <summary>
/// Encodes and decodes the opaque <c>nextCursor</c> string exchanged with clients.
/// </summary>
/// <remarks>
/// A <see cref="FeedCursor"/> is a timestamp and an id. Handing clients those two fields
/// directly would invite them to construct their own, and that would freeze the ordering
/// key as part of the public API - when Phase 2 adds ranked feeds with a different cursor
/// shape, every client would break.
///
/// <para>So it goes over the wire base64url-encoded and documented as opaque. This is
/// obfuscation for API-evolution purposes, <b>not</b> a security measure: anyone can decode
/// it, and nothing depends on them not doing so. It carries no secrets - a creation
/// timestamp and a post id the caller has already seen. Never put anything confidential in
/// a cursor.</para>
/// </remarks>
public static class CursorCodec
{
    /// <summary>Serialises a cursor to its opaque wire form.</summary>
    ///
    /// TODO: Implement.
    /// Suggested format: base64url( "{UtcTicks}|{Id:N}" ).
    /// Use System.Buffers.Text.Base64Url so the value is URL-safe without escaping -
    /// standard base64 produces '+' and '/', which break in query strings.
    public static string Encode(FeedCursor cursor)
        => throw new NotImplementedException();

    /// <summary>
    /// Parses a cursor produced by <see cref="Encode"/>.
    /// </summary>
    /// <returns>True if the value was well formed.</returns>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Return FALSE rather than throwing for null, blank, non-base64, wrong shape, or
    ///     out-of-range ticks. Cursors arrive from query strings, so a malformed one is an
    ///     ordinary bad request - often just a stale bookmark - not an exceptional case.
    ///   - Round-trip property: TryDecode(Encode(c)) must yield exactly c.
    public static bool TryDecode(string? value, out FeedCursor cursor)
        => throw new NotImplementedException();
}
