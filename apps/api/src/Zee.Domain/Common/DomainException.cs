namespace Zee.Domain.Common;

/// <summary>
/// Thrown when an operation would leave an entity in a state the business rules forbid -
/// an empty post body, an event that ends before it starts, a message addressed to yourself.
/// </summary>
/// <remarks>
/// This is the domain's last line of defence, not the primary one. Well-formed requests are
/// rejected earlier by FluentValidation in the Application layer, which can report every
/// problem at once with field names attached. A <see cref="DomainException"/> reaching the
/// API surface means a code path bypassed validation; the exception middleware maps it to
/// HTTP 400 so the caller still gets something sensible rather than a 500.
/// </remarks>
public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
