using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>
/// A campus event students can RSVP to.
/// </summary>
/// <remarks>
/// Unlike <see cref="Competition"/>, <see cref="UniversityId"/> is required. Events have a
/// physical <see cref="Location"/> and a wall-clock time; they belong to a campus by
/// nature. A cross-campus online gathering is better modelled as a competition or a
/// GlobalInterest group post.
/// </remarks>
public sealed class Event : Entity
{
    private readonly List<Rsvp> _rsvps = [];

    /// <summary>Required by EF Core.</summary>
    private Event()
    {
    }

    private Event(
        Guid id,
        string title,
        Guid universityId,
        string location,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? description,
        Guid createdBy)
        : base(id)
    {
        Title = title;
        UniversityId = universityId;
        Location = location;
        StartTime = startTime;
        EndTime = endTime;
        Description = description;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Title { get; private set; } = null!;

    /// <summary>The hosting campus. Required.</summary>
    public Guid UniversityId { get; private set; }

    /// <summary>Free text - a room, a building, a URL for a virtual room.</summary>
    public string Location { get; private set; } = null!;

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset EndTime { get; private set; }

    public string? Description { get; private set; }

    /// <summary>The student who created the listing.</summary>
    public Guid CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public University? University { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Creator { get; private set; }

    /// <summary>Responses to this event.</summary>
    public IReadOnlyCollection<Rsvp> Rsvps => _rsvps.AsReadOnly();

    /// <summary>Creates an event listing.</summary>
    /// <exception cref="DomainException">
    /// If the title or location is blank, or the event does not end after it starts.
    /// </exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - universityId and createdBy must not be Guid.Empty.
    ///   - endTime must be strictly after startTime.
    ///   - Title required, max 200. Location required, max 300. Description optional, max 5000.
    ///   - Times stored as UTC.
    public static Event Create(
        string title,
        Guid universityId,
        string location,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid createdBy,
        string? description = null)
        => throw new NotImplementedException();

    /// <summary>
    /// Records or updates a student's response.
    /// </summary>
    /// <remarks>
    /// Lives on Event rather than on Rsvp because enforcing one-response-per-student
    /// requires seeing the other responses, and Event is the aggregate that holds them.
    /// </remarks>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Responding twice UPDATES the existing RSVP rather than adding a duplicate.
    ///   - status must be a defined enum member; userId must not be Guid.Empty.
    public Rsvp Respond(Guid userId, RsvpStatus status)
        => throw new NotImplementedException();

    /// <summary>Withdraws a student's response entirely.</summary>
    /// <remarks>Withdrawing removes the row; there is no "not going" status. See <see cref="RsvpStatus"/>.</remarks>
    ///
    /// TODO: Implement (removing a non-existent response is a no-op).
    public void WithdrawResponse(Guid userId)
        => throw new NotImplementedException();

    /// <summary>Updates the editable details of an event.</summary>
    ///
    /// TODO: Implement, reusing the same rules as Create.
    public void UpdateDetails(
        string title,
        string location,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? description)
        => throw new NotImplementedException();
}
