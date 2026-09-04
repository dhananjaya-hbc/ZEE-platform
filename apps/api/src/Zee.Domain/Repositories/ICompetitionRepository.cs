using Zee.Domain.Common;
using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="Competition"/>.</summary>
public interface ICompetitionRepository
{
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Browses competitions a student can enter, newest listing first.
    /// </summary>
    /// <param name="universityId">
    /// The reader's campus. Returns competitions open to everyone (null UniversityId)
    /// plus those restricted to this campus - never another university's private listings.
    /// </param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="recruitingTeamsOnly">Restrict to listings actively looking for teammates.</param>
    /// <param name="includePast">When false, competitions whose end date has passed are excluded.</param>
    Task<IReadOnlyList<Competition>> BrowseAsync(
        Guid universityId,
        CompetitionCategory? category,
        bool recruitingTeamsOnly,
        bool includePast,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default);

    void Add(Competition competition);

    void Remove(Competition competition);
}
