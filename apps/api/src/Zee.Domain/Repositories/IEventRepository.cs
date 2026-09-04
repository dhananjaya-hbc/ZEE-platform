using Zee.Domain.Entities;

namespace Zee.Domain.Repositories;

/// <summary>Persistence operations for <see cref="Event"/>.</summary>
public interface IEventRepository
{
    /// <summary>Loads an event without its RSVPs.</summary>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an event together with its RSVP collection.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="GetByIdAsync"/> on purpose. <c>Event.Respond</c> has to see
    /// the existing responses to enforce one-RSVP-per-student, so it needs the collection
    /// loaded; every read-only path would rather not pay for it. Making that explicit at
    /// the interface beats a lazy-loading surprise in a hot path.
    /// </remarks>
    Task<Event?> GetByIdWithRsvpsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists upcoming events on a campus, soonest first.
    /// </summary>
    /// <remarks>
    /// Ordered by <c>StartTime</c> ascending, not by creation. An events list that is not
    /// "what is happening next" is not useful, so this ordering is intentional and differs
    /// from the feed's.
    /// </remarks>
    Task<IReadOnlyList<Event>> GetUpcomingByUniversityAsync(
        Guid universityId,
        DateTimeOffset from,
        int limit,
        CancellationToken cancellationToken = default);

    void Add(Event @event);

    void Remove(Event @event);
}
