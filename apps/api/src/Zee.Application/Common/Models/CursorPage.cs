namespace Zee.Application.Common.Models;

/// <summary>
/// One page of a keyset-paginated listing.
/// </summary>
/// <typeparam name="T">The item type, always a DTO rather than a domain entity.</typeparam>
/// <param name="Items">The page contents, in the listing's order.</param>
/// <param name="NextCursor">
/// Opaque cursor to pass back for the following page, or null when the end has been reached.
/// </param>
/// <remarks>
/// There is no total count, on purpose. <c>COUNT(*)</c> over a feed is an expensive query
/// that gets slower as the platform grows, and an infinite-scroll UI has no use for the
/// number - "is there more" is answered by whether <paramref name="NextCursor"/> is null.
/// </remarks>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor)
{
    /// <summary>Whether another page exists.</summary>
    public bool HasMore => NextCursor is not null;
}
