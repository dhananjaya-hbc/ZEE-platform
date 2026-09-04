using System.Security.Cryptography;
using System.Text;
using Zee.Application.Common.Interfaces;
using System.Globalization;


namespace Zee.Infrastructure.Auth;

/// <summary>HTTP-free implementation of <see cref="IOtpService"/>.</summary>
/// <remarks>
/// SHA-256, not a slow password hash like bcrypt or Argon2. Those exist to make offline
/// cracking expensive for a secret that might live for years (a password). A code here
/// expires in <see cref="Zee.Domain.Entities.EmailVerificationCode.Lifetime"/> and is
/// burned after <see cref="Zee.Domain.Entities.EmailVerificationCode.MaxAttempts"/> wrong
/// guesses, so by the time an offline crack could finish, the digest is worthless anyway.
/// A slow hash here would only add latency with no matching security benefit.
/// </remarks>
public sealed class OtpService : IOtpService
{
    private const int CodeLength = 6;

    public string GenerateCode()
    {
        // Upper bound is exclusive, so this yields 0..999_999 inclusive - exactly the
        // range of every possible 6-digit string.
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);

        // Zero-padded to exactly 6 digits. Without this, "42917" (5 digits) would never
        // match what the student was actually emailed, since the digest is taken of the
        // padded form everywhere else.
        return value.ToString($"D{CodeLength}", CultureInfo.InvariantCulture);
    }

    public string Hash(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexStringLower(hash);
    }
}
