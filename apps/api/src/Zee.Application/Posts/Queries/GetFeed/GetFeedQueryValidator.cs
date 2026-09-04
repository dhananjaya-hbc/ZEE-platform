using FluentValidation;

namespace Zee.Application.Posts.Queries.GetFeed;

/// <summary>Shape validation for <see cref="GetFeedQuery"/>.</summary>
///
/// WARNING: this validator currently enforces NOTHING.
///
/// TODO: Implement the rules.
/// Acceptance criteria:
///   - Limit: InclusiveBetween(1, 50).
///     The upper bound is not cosmetic. Without it, "?limit=100000" is a trivial way to
///     make the server materialise an enormous result set on every request.
///   - Cursor: when supplied, must decode via CursorCodec.TryDecode; reject with a
///     message telling the caller to start a fresh page rather than exposing internals.
public sealed class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    /// <summary>Largest page the feed will serve.</summary>
    public const int MaxLimit = 50;

    public GetFeedQueryValidator()
    {
        // Rules go here. See the acceptance criteria above.
    }
}
