using System.Security.Cryptography;
using System.Text;
using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>
/// A pending one-time password issued to an institutional email address.
/// </summary>
/// <remarks>
/// This is the whole of ZEE's authentication state. There are no passwords; proving you
/// can read mail at a verified campus domain <i>is</i> the credential.
///
/// <para><b>The plaintext code is never stored.</b> Only <see cref="CodeHash"/> is
/// persisted, so a database leak does not hand an attacker a set of live login codes. The
/// Infrastructure layer hashes the code before calling <see cref="Create"/> and hashes the
/// candidate before calling <see cref="Verify"/>; this entity only ever sees digests, which
/// also keeps the choice of algorithm out of the Domain layer.</para>
///
/// <para>A 6-digit code is only a million possibilities, so expiry alone is not enough
/// protection. <see cref="MaxAttempts"/> caps guesses per issued code, and the row is
/// scoped to one email address, so an attacker cannot spread guesses across many codes for
/// the same target.</para>
/// </remarks>
public sealed class EmailVerificationCode : Entity
{
    /// <summary>Wrong guesses allowed before the code is burned.</summary>
    public const int MaxAttempts = 5;

    /// <summary>How long an issued code stays valid.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    /// <summary>Required by EF Core.</summary>
    private EmailVerificationCode()
    {
    }

    private EmailVerificationCode(Guid id, string email, Guid universityId, string codeHash, DateTimeOffset expiresAt)
        : base(id)
    {
        Email = email;
        UniversityId = universityId;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>The address the code was sent to, normalised lowercase.</summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// The university whose allowlist matched this address, resolved at request time so
    /// verification does not have to repeat the domain lookup.
    /// </summary>
    public Guid UniversityId { get; private set; }

    /// <summary>Digest of the code. The plaintext exists only in the student's inbox.</summary>
    public string CodeHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Set when the code is successfully redeemed. Non-null means spent.</summary>
    public DateTimeOffset? ConsumedAt { get; private set; }

    /// <summary>Failed verification attempts so far.</summary>
    public int AttemptCount { get; private set; }

    /// <summary>True once redeemed.</summary>
    public bool IsConsumed => ConsumedAt is not null;

    /// <summary>
    /// Issues a code for an address that has already been matched to a university.
    /// </summary>
    /// <param name="codeHash">
    /// Digest of the code. <b>Never pass the plaintext.</b> Hashing happens in
    /// Infrastructure so the algorithm choice stays out of the Domain layer.
    /// </param>
    public static EmailVerificationCode Create(string email, Guid universityId, string codeHash) =>
        new(NewId(),
            User.NormaliseEmail(email),
            Guard.NotEmpty(universityId),
            Guard.NotEmptyAndAtMost(codeHash, 200),
            DateTimeOffset.UtcNow.Add(Lifetime));

    /// <summary>
    /// Checks a candidate digest, recording the attempt and consuming the code on success.
    /// </summary>
    /// <param name="candidateHash">The digest of the code the student submitted.</param>
    /// <remarks>
    /// Order matters here. Already-used and expired codes are rejected before any
    /// comparison happens, so a spent code cannot be used to keep probing. Comparison uses
    /// <see cref="CryptographicOperations.FixedTimeEquals"/> rather than string equality:
    /// ordinary comparison returns as soon as two bytes differ, and that timing difference
    /// is measurable enough to reconstruct a digest byte by byte.
    /// </remarks>
    public OtpVerificationResult Verify(string candidateHash)
    {
        if (IsConsumed)
        {
            return OtpVerificationResult.AlreadyUsed;
        }

        if (AttemptCount >= MaxAttempts)
        {
            return OtpVerificationResult.TooManyAttempts;
        }

        if (DateTimeOffset.UtcNow >= ExpiresAt)
        {
            return OtpVerificationResult.Expired;
        }

        var expected = Encoding.UTF8.GetBytes(CodeHash);
        var candidate = Encoding.UTF8.GetBytes(candidateHash ?? string.Empty);

        if (!CryptographicOperations.FixedTimeEquals(expected, candidate))
        {
            AttemptCount++;
            return OtpVerificationResult.IncorrectCode;
        }

        ConsumedAt = DateTimeOffset.UtcNow;

        return OtpVerificationResult.Success;
    }
}
