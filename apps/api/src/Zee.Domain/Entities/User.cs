using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>
/// A verified student. Every user belongs to exactly one <see cref="University"/>.
/// </summary>
/// <remarks>
/// There is no password field, by design. Authentication is a one-time code sent to the
/// institutional inbox (see <see cref="EmailVerificationCode"/>), so ZEE never stores a
/// credential a student could reuse elsewhere and never has a password database to leak.
///
/// <para><see cref="Courses"/> and <see cref="Interests"/> are plain string lists in
/// Phase 1 - free text the student types. Phase 2 turns them into matching signals, but
/// the shape here does not need to change for that to happen.</para>
/// </remarks>
public sealed class User : Entity
{
    private readonly List<string> _courses = [];
    private readonly List<string> _interests = [];

    private User()
    {
    }

    private User(Guid id, Guid universityId, string email, string name, string? major, int? year)
        : base(id)
    {
        UniversityId = universityId;
        Email = email;
        Name = name;
        Major = major;
        Year = year;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>The campus this student is verified against. Immutable after creation.</summary>
    public Guid UniversityId { get; private set; }

    /// <summary>
    /// The verified institutional address, stored lowercased. Unique across the platform.
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>Display name.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Field of study. Optional - not every institution uses the concept.</summary>
    public string? Major { get; private set; }

    /// <summary>Year of study, 1-10. Optional.</summary>
    public int? Year { get; private set; }

    /// <summary>Course codes or names the student is taking. PostgreSQL <c>text[]</c>.</summary>
    public IReadOnlyCollection<string> Courses => _courses.AsReadOnly();

    /// <summary>Free-text interests. PostgreSQL <c>text[]</c>.</summary>
    public IReadOnlyCollection<string> Interests => _interests.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Last successful sign-in, for basic activity reporting.</summary>
    public DateTimeOffset? LastSeenAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public University? University { get; private set; }

    /// <summary>
    /// Registers a verified student.
    /// </summary>
    /// <remarks>
    /// The caller is responsible for having already checked the email against the
    /// university's allowlist - this factory validates shape, not membership, because the
    /// Domain layer cannot load the University row itself.
    /// </remarks>
    /// <exception cref="DomainException">If any field is blank or out of range.</exception>
    public static User Create(
        Guid universityId,
        string email,
        string name,
        string? major = null,
        int? year = null)
    {
        return new User(
            NewId(),
            Guard.NotEmpty(universityId),
            NormaliseEmail(email),
            Guard.NotEmptyAndAtMost(name, 100),
            Guard.OptionalAtMost(major, 100),
            ValidateYear(year));
    }

    /// <summary>Updates the parts of a profile a student is allowed to change.</summary>
    public void UpdateProfile(string name, string? major, int? year)
    {
        Name = Guard.NotEmptyAndAtMost(name, 100);
        Major = Guard.OptionalAtMost(major, 100);
        Year = ValidateYear(year);
    }

    /// <summary>Replaces the course list wholesale. Blank entries and duplicates are dropped.</summary>
    public void SetCourses(IEnumerable<string> courses) => ReplaceTags(_courses, courses, 20, 100, "courses");

    /// <summary>Replaces the interest list wholesale. Blank entries and duplicates are dropped.</summary>
    public void SetInterests(IEnumerable<string> interests) => ReplaceTags(_interests, interests, 30, 50, "interests");

    /// <summary>Records a successful sign-in.</summary>
    public void MarkSeen() => LastSeenAt = DateTimeOffset.UtcNow;

    /// <summary>Lowercases and trims an address so lookups and uniqueness behave predictably.</summary>
    public static string NormaliseEmail(string email)
    {
        var normalised = Guard.NotEmptyAndAtMost(email, 254).ToLowerInvariant();
        var atIndex = normalised.LastIndexOf('@');

        if (atIndex <= 0 || atIndex == normalised.Length - 1 ||
            !normalised[(atIndex + 1)..].Contains('.', StringComparison.Ordinal))
        {
            throw new DomainException($"'{email}' is not a valid email address.");
        }

        return normalised;
    }

    private static int? ValidateYear(int? year)
    {
        if (year is null)
        {
            return null;
        }

        if (year is < 1 or > 10)
        {
            throw new DomainException("year must be between 1 and 10.");
        }

        return year;
    }

    private static void ReplaceTags(
        List<string> target,
        IEnumerable<string> values,
        int maxCount,
        int maxLength,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(values);

        var cleaned = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleaned.Count > maxCount)
        {
            throw new DomainException($"{fieldName} may contain at most {maxCount} entries.");
        }

        if (cleaned.Exists(v => v.Length > maxLength))
        {
            throw new DomainException($"Each entry in {fieldName} must be {maxLength} characters or fewer.");
        }

        target.Clear();
        target.AddRange(cleaned);
    }
}
