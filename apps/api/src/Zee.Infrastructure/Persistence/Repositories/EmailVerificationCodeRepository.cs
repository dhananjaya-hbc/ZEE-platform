using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IEmailVerificationCodeRepository"/>.</summary>
public sealed class EmailVerificationCodeRepository(AppDbContext db) : IEmailVerificationCodeRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement.
    /// Normalise the email, filter to ConsumedAt == null and ExpiresAt > UtcNow, then take
    /// the NEWEST by CreatedAt. Must be tracked - Verify() increments AttemptCount and sets
    /// ConsumedAt, and those writes have to persist.
    public Task<EmailVerificationCode?> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement with CountAsync over CreatedAt >= since.
    /// This backs the rate limit on the request-code endpoint. Without it, that endpoint is
    /// an open mail relay aimed at any institutional inbox an attacker cares to name.
    public Task<int> CountIssuedSinceAsync(
        string email,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(EmailVerificationCode code) => _db.EmailVerificationCodes.Add(code);

    /// TODO: Implement with ExecuteDeleteAsync - a set-based delete, not a load-then-remove
    /// loop. This runs as a background job over potentially large numbers of rows.
    public Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
