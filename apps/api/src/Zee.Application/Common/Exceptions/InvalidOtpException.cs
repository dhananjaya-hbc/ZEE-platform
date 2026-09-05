namespace Zee.Application.Common.Exceptions;

/// <summary>
/// Thrown when an OTP code fails verification, for any reason. Mapped to HTTP 400.
/// </summary>
/// <remarks>
/// <b>Deliberately one message for every failure case.</b> Wrong code, expired code,
/// already-used code, and "no code was ever requested for this address" all produce the
/// exact same response. See <see cref="Zee.Domain.Enums.OtpVerificationResult"/>'s own
/// remarks: telling an anonymous caller <i>which way</i> their guess was wrong hands them
/// a free oracle for probing the system. The distinct failure reasons still exist - they
/// are just never surfaced past this exception's fixed message.
/// </remarks>
public sealed class InvalidOtpException : Exception
{
    public InvalidOtpException()
        : base("Invalid or expired code.")
    {
    }
}
