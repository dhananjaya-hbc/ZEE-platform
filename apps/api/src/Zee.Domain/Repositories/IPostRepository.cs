using Zee.Domain.Common;
using Zee.Domain.Entities;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="Post"/>.</summary>
public interface IPostRepository
{
    /// <summary>Loads a single post, or null if it does not exist.</summary>
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one page of the reverse-chronological feed visible to a student.
    /// </summary>
    /// <param name="userId">The reader. Determines group membership visibility.</param>
    /// <param name="universityId">The reader's campus, for University-visibility posts.</param>
    /// <param name="cursor">Position to read from, or null to start at the newest post.</param>
    /// <param name="limit">Maximum posts to return.</param>
    /// <remarks>
    /// <b>Phase 1 ordering is purely <c>CreatedAt DESC, Id DESC</c>.</b> No engagement
    /// signal, no personalisation, no scoring. Ranking arrives in Phase 2 through the AI
    /// service, and it will not replace this method - the ranked path fetches ids from the
    /// AI service and hydrates them via <see cref="GetByIdsAsync"/>, leaving chronological
    /// as the fallback whenever the AI service is unavailable.
    /// </remarks>
    Task<IReadOnlyList<Post>> GetChronologicalFeedAsync(
        Guid userId,
        Guid universityId,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads posts by id, preserving the order of <paramref name="ids"/>.
    /// </summary>
    /// <remarks>
    /// Present in Phase 1 for a Phase 2 reason: the AI feed endpoint returns
    /// <c>{ post_id, score }</c> pairs already in rank order, and the caller needs the full
    /// posts back in exactly that order. Databases do not preserve <c>WHERE id IN (...)</c>
    /// ordering, so the implementation re-sorts to match the input.
    /// </remarks>
    Task<IReadOnlyList<Post>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Returns one page of a single student's posts, newest first.</summary>
    Task<IReadOnlyList<Post>> GetByAuthorAsync(
        Guid authorId,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>Stages a new post. Not written until <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Post post);

    /// <summary>Stages a deletion. Not applied until <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Remove(Post post);
}
