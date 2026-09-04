using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>
/// Join entity linking a <see cref="User"/> to a <see cref="Group"/>.
/// </summary>
/// <remarks>
/// This is a first-class entity rather than an implicit many-to-many table because
/// membership carries its own facts - when the student joined, and whether they moderate
/// the group. EF Core can hide a bare join table, but the moment you need a column on the
/// relationship you want the entity, and moderation is coming.
/// </remarks>
public sealed class GroupMembership : Entity
{
    private GroupMembership()
    {
    }

    private GroupMembership(Guid id, Guid groupId, Guid userId, bool isModerator)
        : base(id)
    {
        GroupId = groupId;
        UserId = userId;
        IsModerator = isModerator;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public Guid GroupId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Whether this member can moderate the group. The creator starts as one.</summary>
    public bool IsModerator { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public Group? Group { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? User { get; private set; }

    internal static GroupMembership Create(Guid groupId, Guid userId, bool isModerator = false) =>
        new(NewId(), Guard.NotEmpty(groupId), Guard.NotEmpty(userId), isModerator);

    /// <summary>Grants moderator rights.</summary>
    public void PromoteToModerator() => IsModerator = true;

    /// <summary>Revokes moderator rights.</summary>
    public void DemoteToMember() => IsModerator = false;
}
