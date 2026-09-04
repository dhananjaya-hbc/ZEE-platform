using Zee.Domain.Common;

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

    /// <exception cref="DomainException">
    /// If the title or location is blank, or the event does not end after it starts.
    /// </exception>
    public static Event Create(
        string title,
        Guid universityId,
        string location,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Guid createdBy,
        string? description = null)
    {
        Guard.NotEmpty(universityId);
        Guard.NotEmpty(createdBy);
        Guard.EndAfterStart(startTime, endTime, "An event");

        return new Event(
            NewId(),
            Guard.NotEmptyAndAtMost(title, 200),
            universityId,
            Guard.NotEmptyAndAtMost(location, 300),
            startTime.ToUniversalTime(),
            endTime.ToUniversalTime(),
            Guard.OptionalAtMost(description, 5000),
            createdBy);
    }

    /// <summary>
    /// Records or updates a student's response. Answering twice updates the existing RSVP
    /// rather than creating a duplicate.
    /// </summary>
    public Rsvp Respond(Guid userId, Enums.RsvpStatus status)
    {
        Guard.NotEmpty(userId);
        Guard.DefinedEnum(status);

        var existing = _rsvps.Find(r => r.UserId == userId);

        if (existing is not null)
        {
            existing.ChangeStatus(status);
            return existing;
        }

        var rsvp = Rsvp.Create(Id, userId, status);
        _rsvps.Add(rsvp);

        return rsvp;
    }

    /// <summary>Withdraws a student's response entirely.</summary>
    public void WithdrawResponse(Guid userId)
    {
        var existing = _rsvps.Find(r => r.UserId == userId);

        if (existing is not null)
        {
            _rsvps.Remove(existing);
        }
    }

    public void UpdateDetails(
        string title,
        string location,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? description)
    {
        Guard.EndAfterStart(startTime, endTime, "An event");

        Title = Guard.NotEmptyAndAtMost(title, 200);
        Location = Guard.NotEmptyAndAtMost(location, 300);
        StartTime = startTime.ToUniversalTime();
        EndTime = endTime.ToUniversalTime();
        Description = Guard.OptionalAtMost(description, 5000);
    }
}
