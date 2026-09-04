using Zee.Domain.Entities;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="EmailVerificationCode"/>.</summary>
public interface IEmailVerificationCodeRepository
{
    /// <summary>
    /// Returns the most recently issued, still-live code for an address, or null.
    /// </summary>
    /// <remarks>
    /// Only the newest code is considered valid. Requesting a fresh code therefore
    /// invalidates the previous one in practice, which is what students expect ("I asked
    /// again, use the new one") and also shrinks the window in which several codes for the
    /// same inbox are simultaneously guessable.
    /// </remarks>
    Task<EmailVerificationCode?> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts codes issued to an address since a given instant, for request rate limiting.
    /// </summary>
    /// <remarks>
    /// Without this, the request-code endpoint is an open mail relay pointed at any
    /// institutional inbox an attacker names.
    /// </remarks>
    Task<int> CountIssuedSinceAsync(
        string email,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    void Add(EmailVerificationCode code);

    /// <summary>
    /// Deletes codes that expired before <paramref name="olderThan"/>.
    /// </summary>
    /// <remarks>Called by a background job; spent codes have no reason to accumulate.</remarks>
    Task<int> PurgeExpiredAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default);
}
