using Zee.Domain.Entities;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="User"/>.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks a student up by institutional address. The caller does not need to normalise
    /// case first - the implementation matches on the stored lowercase form.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Whether an address is already registered. Cheaper than loading the row.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Loads users by id, for hydrating AI recommendation results in Phase 2.</summary>
    Task<IReadOnlyList<User>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the ids of every group the student belongs to.</summary>
    Task<IReadOnlyList<Guid>> GetGroupIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(User user);
}
