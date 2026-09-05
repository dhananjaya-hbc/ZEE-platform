namespace Zee.Application.Common.Exceptions;

/// <summary>
/// Thrown when a caller has exceeded a rate limit. Mapped to HTTP 429.
/// </summary>
/// <remarks>
/// Used by <c>RequestOtpCommand</c> to cap how many codes can be requested for one
/// address in a short window. Without this, the endpoint that emails a login code is an
/// open mail relay pointed at any institutional inbox an attacker cares to name.
/// </remarks>
public sealed class TooManyRequestsException : Exception
{
    public TooManyRequestsException()
        : base("Too many requests. Please try again later.")
    {
    }

    public TooManyRequestsException(string message)
        : base(message)
    {
    }

    public TooManyRequestsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
