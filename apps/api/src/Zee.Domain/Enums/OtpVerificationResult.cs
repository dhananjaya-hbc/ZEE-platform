namespace Zee.Domain.Enums;

/// <summary>The outcome of checking a submitted one-time code.</summary>
/// <remarks>
/// The API deliberately collapses every failure case into one generic message for the
/// caller. These distinct values exist for logging, metrics and rate-limit decisions on
/// the server side - telling an anonymous caller <i>which</i> way their guess was wrong
/// hands them a free oracle.
/// </remarks>
public enum OtpVerificationResult
{
    /// <summary>The code matched and has now been consumed.</summary>
    Success = 0,

    /// <summary>The code did not match. The attempt has been counted.</summary>
    IncorrectCode = 1,

    /// <summary>The code was correct at some point but has passed its expiry.</summary>
    Expired = 2,

    /// <summary>This code was already redeemed. Codes are single-use.</summary>
    AlreadyUsed = 3,

    /// <summary>Too many wrong guesses; this code is now dead regardless of correctness.</summary>
    TooManyAttempts = 4,
}
