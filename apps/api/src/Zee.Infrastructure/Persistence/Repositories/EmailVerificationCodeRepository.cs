using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IEmailVerificationCodeRepository"/>.</summary>
public sealed class EmailVerificationCodeRepository(AppDbContext db) : IEmailVerificationCodeRepository
{
    private readonly AppDbContext _db = db;

     /// <summary>
    /// Returns the most recently issued, still-live code for an address, or null.
    /// </summary>
    /// <remarks>
    /// Returns a TRACKED entity - EmailVerificationCode.Verify() mutates AttemptCount and
    /// ConsumedAt, and IUnitOfWork.SaveChangesAsync needs to see those changes to persist them.
    /// </remarks>
    public async Task<EmailVerificationCode?> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalised = User.NormaliseEmail(email);
        var now = DateTimeOffset.UtcNow;

        return await _db.EmailVerificationCodes
            .Where(c => c.Email == normalised && c.ConsumedAt == null && c.ExpiresAt > now)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Counts codes issued to an address since a given instant, for request rate limiting.
    /// </summary>
    public async Task<int> CountIssuedSinceAsync(
        string email,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        var normalised = User.NormaliseEmail(email);

        return await _db.EmailVerificationCodes
            .CountAsync(c => c.Email == normalised && c.CreatedAt >= since, cancellationToken);
    }

    public void Add(EmailVerificationCode code) => _db.EmailVerificationCodes.Add(code);

    /// TODO: Implement with ExecuteDeleteAsync - a set-based delete, not a load-then-remove
    /// loop. This runs as a background job over potentially large numbers of rows.
    public Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
