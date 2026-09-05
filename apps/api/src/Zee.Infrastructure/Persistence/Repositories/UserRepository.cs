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

    /// <summary>
    /// Looks a student up by institutional address. Returns a TRACKED entity - callers
    /// (VerifyOtpCommandHandler in particular) mutate it via MarkSeen() and rely on
    /// IUnitOfWork.SaveChangesAsync to persist that, which requires tracking.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalised = User.NormaliseEmail(email);

        return await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalised, cancellationToken);
    }

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
