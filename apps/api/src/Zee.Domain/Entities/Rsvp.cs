using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>A student's response to an <see cref="Entities.Event"/>.</summary>
/// <remarks>
/// Created through <see cref="Entities.Event.Respond"/> rather than directly, so the
/// "one RSVP per student per event" rule is enforced by the aggregate that can actually
/// see the other responses. A unique index on (EventId, UserId) backs it up in the database.
/// </remarks>
public sealed class Rsvp : Entity
{
    private Rsvp()
    {
    }

    private Rsvp(Guid id, Guid eventId, Guid userId, RsvpStatus status)
        : base(id)
    {
        EventId = eventId;
        UserId = userId;
        Status = status;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; private set; }

    public Guid UserId { get; private set; }

    public RsvpStatus Status { get; private set; }

    /// <summary>When the response was last set or changed.</summary>
    public DateTimeOffset RespondedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public Event? Event { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? User { get; private set; }

    internal static Rsvp Create(Guid eventId, Guid userId, RsvpStatus status) =>
        new(NewId(), Guard.NotEmpty(eventId), Guard.NotEmpty(userId), Guard.DefinedEnum(status));

    internal void ChangeStatus(RsvpStatus status)
    {
        Status = Guard.DefinedEnum(status);
        RespondedAt = DateTimeOffset.UtcNow;
    }
}
