using Microsoft.EntityFrameworkCore;
using Zee.Domain.Common;
using Zee.Domain.Entities;
using Zee.Domain.Enums;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICompetitionRepository"/>.</summary>
public sealed class CompetitionRepository(AppDbContext db) : ICompetitionRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement. Include Organizer for the DTO's display name.
    public Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Scope filter, always applied:
    ///       UniversityId == null (open to everyone) OR UniversityId == universityId.
    ///     Never return another campus's restricted listings.
    ///   - category / recruitingTeamsOnly are optional filters, applied only when set.
    ///   - includePast == false excludes rows whose EndDate has passed.
    ///   - Same keyset pagination pattern as PostRepository (CreatedAt DESC, Id DESC).
    public Task<IReadOnlyList<Competition>> BrowseAsync(
        Guid universityId,
        CompetitionCategory? category,
        bool recruitingTeamsOnly,
        bool includePast,
        FeedCursor? cursor,
        int limit,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(Competition competition) => _db.Competitions.Add(competition);

    public void Remove(Competition competition) => _db.Competitions.Remove(competition);
}
