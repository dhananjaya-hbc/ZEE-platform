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
    /// Creates a group. The creator is enrolled immediately as its first moderator, so a
    /// group is never left without anyone able to administer it.
    /// </summary>
    /// <exception cref="DomainException">
    /// If the name is blank, or the type and campus scope contradict each other.
    /// </exception>
    public static Group Create(
        string name,
        GroupType type,
        Guid creatorId,
        Guid? universityId = null,
        string? description = null)
    {
        Guard.DefinedEnum(type);
        Guard.NotEmpty(creatorId);

        if (type == GroupType.GlobalInterest && universityId is not null)
        {
            throw new DomainException(
                "A GlobalInterest group spans campuses and must not specify a universityId.");
        }

        if (type != GroupType.GlobalInterest && universityId is null)
        {
            throw new DomainException($"A {type} group must specify the universityId it belongs to.");
        }

        if (universityId is not null)
        {
            Guard.NotEmpty(universityId.Value, nameof(universityId));
        }

        var group = new Group(
            NewId(),
            Guard.NotEmptyAndAtMost(name, 120),
            type,
            universityId,
            Guard.OptionalAtMost(description, 1000));

        group._members.Add(GroupMembership.Create(group.Id, creatorId, isModerator: true));

        return group;
    }

    /// <summary>Adds a member. Joining twice is a no-op rather than an error.</summary>
    public GroupMembership AddMember(Guid userId, bool isModerator = false)
    {
        Guard.NotEmpty(userId);

        var existing = _members.Find(m => m.UserId == userId);

        if (existing is not null)
        {
            return existing;
        }

        var membership = GroupMembership.Create(Id, userId, isModerator);
        _members.Add(membership);

        return membership;
    }

    /// <summary>
    /// Removes a member. Refuses to remove the last moderator, which would leave the group
    /// unadministrable.
    /// </summary>
    public void RemoveMember(Guid userId)
    {
        var membership = _members.Find(m => m.UserId == userId);

        if (membership is null)
        {
            return;
        }

        if (membership.IsModerator && _members.Count(m => m.IsModerator) == 1)
        {
            throw new DomainException("A group must keep at least one moderator.");
        }

        _members.Remove(membership);
    }

    /// <summary>Whether the given student is a member.</summary>
    public bool HasMember(Guid userId) => _members.Exists(m => m.UserId == userId);

    public void UpdateDetails(string name, string? description)
    {
        Name = Guard.NotEmptyAndAtMost(name, 120);
        Description = Guard.OptionalAtMost(description, 1000);
    }
}
