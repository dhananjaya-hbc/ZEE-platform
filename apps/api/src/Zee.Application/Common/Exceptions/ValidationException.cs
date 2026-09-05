using FluentValidation.Results;

namespace Zee.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request fails FluentValidation, carrying every failure at once.
/// </summary>
/// <remarks>
/// Errors are grouped by property name so the API can return an RFC 9457 problem document
/// with a populated <c>errors</c> object, and the web client can attach each message to the
/// field it belongs to. Reporting one failure at a time would make a student fix a form
/// through several round trips.
/// </remarks>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        ArgumentNullException.ThrowIfNull(failures);

        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
    }

    /// <summary>Failure messages keyed by the property that produced them.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
