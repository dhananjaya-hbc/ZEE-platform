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

    /// <summary>Required by EF Core.</summary>
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

    /// <summary>The verified institutional address, stored lowercased. Unique across the platform.</summary>
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
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - universityId must not be Guid.Empty.
    ///   - Email normalised via NormaliseEmail.
    ///   - Name required, trimmed, max 100. Major optional, max 100.
    ///   - Year null or 1..10; anything else throws.
    public static User Create(
        Guid universityId,
        string email,
        string name,
        string? major = null,
        int? year = null)
        => throw new NotImplementedException();

    /// <summary>Updates the parts of a profile a student is allowed to change.</summary>
    /// <remarks>Email and UniversityId are deliberately absent - both are verification facts.</remarks>
    ///
    /// TODO: Implement using the same rules as Create.
    public void UpdateProfile(string name, string? major, int? year)
        => throw new NotImplementedException();

    /// <summary>Replaces the course list wholesale. Blank entries and duplicates are dropped.</summary>
    ///
    /// TODO: Implement (max 20 entries, each max 100 chars).
    public void SetCourses(IEnumerable<string> courses)
        => throw new NotImplementedException();

    /// <summary>Replaces the interest list wholesale. Blank entries and duplicates are dropped.</summary>
    ///
    /// TODO: Implement (max 30 entries, each max 50 chars).
    public void SetInterests(IEnumerable<string> interests)
        => throw new NotImplementedException();

    /// <summary>Records a successful sign-in.</summary>
    public void MarkSeen() => LastSeenAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// Lowercases and trims an address so lookups and uniqueness behave predictably.
    /// </summary>
    /// <remarks>
    /// Public and static because the auth flow needs to normalise an address before a User
    /// exists to call it on - <see cref="EmailVerificationCode"/> uses it too. Getting this
    /// inconsistent is how you end up with two accounts for one inbox.
    /// </remarks>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Trim, lowercase, max 254 chars (RFC 5321).
    ///   - Reject anything without a local part, an '@', or a dot in the domain.
    public static string NormaliseEmail(string email)
        => throw new NotImplementedException();
}
