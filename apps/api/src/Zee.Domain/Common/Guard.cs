using System.Runtime.CompilerServices;

namespace Zee.Domain.Common;

/// <summary>
/// Small argument-checking helpers used by entity factory methods.
/// </summary>
/// <remarks>
/// Every method must throw <see cref="DomainException"/> rather than
/// <c>ArgumentException</c>, because from the caller's perspective these are broken
/// business rules, not programming errors, and the API middleware translates them to
/// HTTP 400 on that basis.
///
/// <para>The <c>[CallerArgumentExpression]</c> attribute means the field name in the error
/// message comes for free: <c>Guard.NotEmpty(content)</c> should report "content must not
/// be empty" with no duplicated string literal to drift out of date.</para>
/// </remarks>
///
/// TODO: Implement every method in this class. Good first issue - self-contained,
/// no dependencies, and easy to cover with tests.
///
/// Acceptance criteria:
///   - Every failure throws DomainException with a message naming the offending field.
///   - String helpers return the TRIMMED value; callers rely on that for normalisation.
///   - OptionalAtMost / OptionalAbsoluteUrl return null (not "") for blank input.
///   - Unit tests live in tests/Zee.Domain.UnitTests/Common/GuardTests.cs.
internal static class Guard
{
    /// <summary>Requires a non-null, non-blank string and returns it trimmed.</summary>
    /// <exception cref="DomainException">If the value is null, empty or whitespace.</exception>
    public static string NotEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        => throw new NotImplementedException();

    /// <summary>Requires a non-blank string no longer than <paramref name="maxLength"/>.</summary>
    /// <exception cref="DomainException">If the value is blank or too long.</exception>
    public static string NotEmptyAndAtMost(
        string? value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        => throw new NotImplementedException();

    /// <summary>
    /// Allows null or blank, but caps the length of anything present.
    /// Returns null for blank input so optional fields normalise to a single "absent" form.
    /// </summary>
    /// <exception cref="DomainException">If a non-blank value exceeds the limit.</exception>
    public static string? OptionalAtMost(
        string? value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        => throw new NotImplementedException();

    /// <summary>Requires a foreign key or identifier that is not <see cref="Guid.Empty"/>.</summary>
    /// <exception cref="DomainException">If the value is <see cref="Guid.Empty"/>.</exception>
    public static Guid NotEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        => throw new NotImplementedException();

    /// <summary>
    /// Requires that a defined enum member was supplied, not an arbitrary cast integer.
    /// </summary>
    /// <remarks>
    /// C# lets <c>(PostVisibility)99</c> through the type system silently, so anything
    /// arriving from JSON needs this check before it reaches the database.
    /// </remarks>
    /// <exception cref="DomainException">If the value is not a declared member.</exception>
    public static TEnum DefinedEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        where TEnum : struct, Enum
        => throw new NotImplementedException();

    /// <summary>Requires that an end instant is strictly after its start.</summary>
    /// <param name="subject">Sentence subject for the message, e.g. "An event".</param>
    /// <exception cref="DomainException">If <paramref name="end"/> is not after <paramref name="start"/>.</exception>
    public static void EndAfterStart(
        DateTimeOffset start,
        DateTimeOffset end,
        string subject)
        => throw new NotImplementedException();

    /// <summary>
    /// Requires an absolute http/https URL, or null. Returns null for blank input.
    /// </summary>
    /// <remarks>
    /// The scheme check is the point: these URLs get rendered as links, and permitting
    /// <c>javascript:</c> or <c>data:</c> here would put a stored-XSS vector into every
    /// profile page.
    /// </remarks>
    /// <exception cref="DomainException">If the value is not an absolute http/https URL.</exception>
    public static string? OptionalAbsoluteUrl(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        => throw new NotImplementedException();
}
