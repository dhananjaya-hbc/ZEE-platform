using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement.
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    /// Normalise the address with User.NormaliseEmail before comparing. Do NOT use
    /// ToLower() in the LINQ query - that produces a lower(email) call PostgreSQL cannot
    /// serve from the plain unique index, turning every sign-in into a sequential scan.
    /// Emails are stored already-normalised precisely so this can be a direct equality match.
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement with AnyAsync - cheaper than loading the row.
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement, preserving the input order like PostRepository.GetByIdsAsync.
    public Task<IReadOnlyList<User>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    /// Query GroupMemberships directly and project to GroupId - do not load Group entities
    /// just to read their ids. This runs on every feed request, so it should touch one
    /// index and return nothing but GUIDs.
    public Task<IReadOnlyList<Guid>> GetGroupIdsAsync(Guid userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(User user) => _db.Users.Add(user);
}
