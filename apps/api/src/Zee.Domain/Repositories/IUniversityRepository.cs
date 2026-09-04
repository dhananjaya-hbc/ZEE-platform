using Zee.Domain.Entities;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="University"/>.</summary>
/// <remarks>
/// Not in the original interface list, but the OTP flow cannot work without it: step one
/// of signup is resolving an email domain to an onboarded campus.
/// </remarks>
public interface IUniversityRepository
{
    Task<University?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the active university that has claimed the domain of the given address, or
    /// null when no onboarded campus matches.
    /// </summary>
    /// <remarks>
    /// This is the gate on the entire platform. A null return is what stops someone signing
    /// up with a personal address, so the implementation matches domains exactly and skips
    /// deactivated universities.
    /// </remarks>
    Task<University?> FindByEmailDomainAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Lists onboarded campuses, for the public "which universities are on ZEE" view.</summary>
    Task<IReadOnlyList<University>> GetActiveAsync(CancellationToken cancellationToken = default);

    void Add(University university);
}
