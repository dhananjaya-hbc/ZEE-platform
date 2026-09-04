namespace Zee.Domain.Common;

/// <summary>
/// Position in a reverse-chronological listing, used for keyset pagination.
/// </summary>
/// <param name="CreatedAt">Creation instant of the last item the caller received.</param>
/// <param name="Id">Id of that same item, breaking ties within the same millisecond.</param>
/// <remarks>
/// Feeds are paged by "everything older than this item", not by <c>OFFSET</c>. Two reasons:
///
/// <list type="number">
/// <item><description><b>Correctness.</b> New posts arrive constantly. With <c>OFFSET 20</c>,
/// anything posted between page 1 and page 2 shifts every row down, so the reader sees an
/// item twice and misses another. A keyset cursor describes a fixed point in the ordering,
/// so pages stay consistent no matter what arrives in between.</description></item>
/// <item><description><b>Cost.</b> <c>OFFSET n</c> makes PostgreSQL walk and discard n rows,
/// so deep pages get linearly slower. A keyset seek jumps straight to the position via the
/// index and stays flat.</description></item>
/// </list>
///
/// <para>The <see cref="Id"/> component is not optional. Sorting on a timestamp alone is
/// not a total order - two posts can share a millisecond - and an unstable sort makes
/// pagination drop or repeat rows at page boundaries.</para>
/// </remarks>
public readonly record struct FeedCursor(DateTimeOffset CreatedAt, Guid Id);
