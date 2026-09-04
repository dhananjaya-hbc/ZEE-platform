using System.Runtime.CompilerServices;

namespace Zee.Domain.Common;

/// <summary>
/// Small argument-checking helpers used by entity factory methods.
/// </summary>
/// <remarks>
/// Every method throws <see cref="DomainException"/> rather than <c>ArgumentException</c>,
/// because from the caller's perspective these are broken business rules, not programming
/// errors, and the API middleware translates them to 400 on that basis.
///
/// <para>The <c>[CallerArgumentExpression]</c> attribute means the field name in the error
/// message comes for free: <c>Guard.NotEmpty(content)</c> reports "content must not be
/// empty" with no duplicated string literal to drift out of date.</para>
/// </remarks>
internal static class Guard
{
    /// <summary>Requires a non-null, non-blank string and returns it trimmed.</summary>
    public static string NotEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{name} must not be empty.");
        }

        return value.Trim();
    }

    /// <summary>Requires a non-blank string no longer than <paramref name="maxLength"/>.</summary>
    public static string NotEmptyAndAtMost(
        string? value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        var trimmed = NotEmpty(value, name);

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{name} must be {maxLength} characters or fewer.");
        }

        return trimmed;
    }

    /// <summary>Allows null or blank, but caps the length of anything present. Returns null for blank.</summary>
    public static string? OptionalAtMost(
        string? value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{name} must be {maxLength} characters or fewer.");
        }

        return trimmed;
    }

    /// <summary>Requires a foreign key or identifier that is not <see cref="Guid.Empty"/>.</summary>
    public static Guid NotEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{name} must be a valid identifier.");
        }

        return value;
    }

    /// <summary>Requires that a defined enum member was supplied, not an arbitrary cast integer.</summary>
    public static TEnum DefinedEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException($"{name} is not a recognised {typeof(TEnum).Name} value.");
        }

        return value;
    }

    /// <summary>Requires that an end instant is strictly after its start.</summary>
    public static void EndAfterStart(
        DateTimeOffset start,
        DateTimeOffset end,
        string subject)
    {
        if (end <= start)
        {
            throw new DomainException($"{subject} must end after it starts.");
        }
    }

    /// <summary>Requires an absolute http/https URL, or null. Returns null for blank input.</summary>
    public static string? OptionalAbsoluteUrl(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException($"{name} must be an absolute http or https URL.");
        }

        return trimmed;
    }
}
