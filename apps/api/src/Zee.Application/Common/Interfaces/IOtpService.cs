namespace Zee.Application.Common.Interfaces;

/// <summary>
/// Generates and hashes one-time passwords for institutional email verification.
/// </summary>
/// <remarks>
/// Declared here, implemented in Infrastructure — the same pattern as
/// <see cref="IAiServiceClient"/>. Keeping this behind an interface means the Domain
/// layer's <c>EmailVerificationCode</c> never has to know what hashing algorithm is in
/// use; it only ever sees the digest this service produces.
///
/// <para><b>Generation must use a cryptographically secure random source</b>
/// (<c>System.Security.Cryptography.RandomNumberGenerator</c>), never
/// <c>System.Random</c>. <c>Random</c> is seeded from a predictable source and is not
/// safe for anything where an attacker benefits from predicting the next value — which
/// describes a login code exactly.</para>
/// </remarks>
public interface IOtpService
{
    /// <summary>Generates a new 6-digit code, e.g. "042917". Leading zeros are preserved.</summary>
    string GenerateCode();

    /// <summary>
    /// Hashes a code (or candidate) for storage or comparison.
    /// </summary>
    /// <remarks>
    /// Called twice per verification cycle: once in Infrastructure right after
    /// <see cref="GenerateCode"/>, to produce the digest that gets stored, and once again
    /// on the candidate a student submits, so <c>EmailVerificationCode.Verify</c> never
    /// sees a plaintext code — only two digests it compares.
    /// </remarks>
    string Hash(string code);
}
