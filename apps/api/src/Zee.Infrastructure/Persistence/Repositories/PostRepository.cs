using Microsoft.EntityFrameworkCore;
using Zee.Domain.Common;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IPostRepository"/>.</summary>
/// <remarks>
/// Add and Remove only stage changes on the change tracker. Nothing reaches PostgreSQL
/// until the handler calls <c>IUnitOfWork.SaveChangesAsync</c>.
/// </remarks>
public sealed class PostRepository(AppDbContext db) : IPostRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement. Include Author and Group so PostDto can fill in display names.
    public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement. This is the single most important query in Phase 1.
    ///
    /// Acceptance criteria:
    ///   1. Start from _db.Posts.AsNoTracking() - the feed is read-only, and change
    ///      tracking on every post in every page is pure overhead.
    ///   2. Visibility filter, applied IN SQL (never with .ToList() first):
    ///        - Visibility == Global, OR
    ///        - Visibility == University AND the author is at universityId, OR
    ///        - Visibility == Group     AND GroupId is in the reader's group ids.
    ///      Filtering in memory returns short pages and eventually empty ones while posts
    ///      still exist, because the limit is applied before the filter.
    ///   3. Keyset predicate when cursor is not null:
    ///        CreatedAt < cursor.CreatedAt
    ///        || (CreatedAt == cursor.CreatedAt && Id < cursor.Id)
    ///      Both halves are required; the second is what makes ties deterministic.
    ///   4. OrderByDescending(CreatedAt).ThenByDescending(Id) - must match the composite
    ///      index in PostConfiguration exactly, or PostgreSQL sorts instead of seeking.
    ///   5. Take(limit). Include Author and Group.
    public Task<IReadOnlyList<Post>> GetChronologicalFeedAsync(
        Guid userId,
        Guid universityId,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    /// The database does not preserve "WHERE id IN (...)" ordering, so re-sort the results
    /// to match the input order before returning - callers (the Phase 2 ranked feed) depend
    /// on rank order surviving the round trip. Ids with no matching row are dropped.
    public Task<IReadOnlyList<Post>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement. Same keyset pattern as the feed, filtered to one author.
    public Task<IReadOnlyList<Post>> GetByAuthorAsync(
        Guid authorId,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(Post post) => _db.Posts.Add(post);

    public void Remove(Post post) => _db.Posts.Remove(post);
}
