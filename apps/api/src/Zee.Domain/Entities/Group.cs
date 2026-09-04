using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>
/// A community: a course cohort, a club, a dorm, or a cross-campus interest group.
/// </summary>
/// <remarks>
/// The type/scope rule lives here rather than in a validator because it is structural, not
/// a request-shape concern: Course, Club and Dorm groups exist at exactly one university,
/// while GlobalInterest groups are the mechanism by which ZEE is global at all. A "Course"
/// group with no campus, or a "GlobalInterest" group pinned to one, would each break a
/// different part of the product.
/// </remarks>
public sealed class Group : Entity
{
    private readonly List<GroupMembership> _members = [];

    /// <summary>Required by EF Core.</summary>
    private Group()
    {
    }

    private Group(Guid id, string name, GroupType type, Guid? universityId, string? description)
        : base(id)
    {
        Name = name;
        Type = type;
        UniversityId = universityId;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = null!;

    public GroupType Type { get; private set; }

    /// <summary>
    /// The owning campus. Required for Course, Club and Dorm; always null for GlobalInterest.
    /// </summary>
    public Guid? UniversityId { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public University? University { get; private set; }

    /// <summary>Members of this group, as membership records.</summary>
    public IReadOnlyCollection<GroupMembership> Members => _members.AsReadOnly();

    /// <summary>
    /// Creates a group and enrols the creator as its first moderator.
    /// </summary>
    /// <exception cref="DomainException">
    /// If the name is blank, or the type and campus scope contradict each other.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - type must be a defined enum member; creatorId must not be Guid.Empty.
    ///   - GlobalInterest + non-null universityId  -> throw.
    ///   - Any other type + null universityId      -> throw.
    ///   - Name required, trimmed, max 120. Description optional, max 1000.
    ///   - The creator is added immediately as a moderator, so a group is never left with
    ///     nobody able to administer it.
    public static Group Create(
        string name,
        GroupType type,
        Guid creatorId,
        Guid? universityId = null,
        string? description = null)
        => throw new NotImplementedException();

    /// <summary>Adds a member. Joining twice must be a no-op rather than an error.</summary>
    /// <returns>The new membership, or the existing one if already a member.</returns>
    ///
    /// TODO: Implement.
    public GroupMembership AddMember(Guid userId, bool isModerator = false)
        => throw new NotImplementedException();

    /// <summary>Removes a member.</summary>
    /// <remarks>
    /// Must refuse to remove the last remaining moderator - that would leave the group
    /// permanently unadministrable, with no path to recovery through the API.
    /// </remarks>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Removing a non-member is a no-op.
    ///   - Removing the sole moderator throws DomainException.
    public void RemoveMember(Guid userId)
        => throw new NotImplementedException();

    /// <summary>Whether the given student is a member.</summary>
    ///
    /// TODO: Implement.
    public bool HasMember(Guid userId)
        => throw new NotImplementedException();

    /// <summary>Updates the editable details of a group.</summary>
    /// <remarks><see cref="Type"/> and <see cref="UniversityId"/> are fixed at creation.</remarks>
    ///
    /// TODO: Implement.
    public void UpdateDetails(string name, string? description)
        => throw new NotImplementedException();
}
