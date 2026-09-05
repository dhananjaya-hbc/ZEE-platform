using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IEventRepository"/>.</summary>
public sealed class EventRepository(AppDbContext db) : IEventRepository
{
    private readonly AppDbContext _db = db;

    /// TODO: Implement, WITHOUT including Rsvps.
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement with .Include(e => e.Rsvps).
    /// Must be TRACKED (no AsNoTracking): Event.Respond mutates the RSVP collection, and
    /// an untracked graph would let the change silently fail to persist.
    public Task<Event?> GetByIdWithRsvpsAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// TODO: Implement.
    /// Filter to universityId and StartTime >= from, then order by StartTime ASCENDING -
    /// a campus calendar is "what is next", which is the opposite of the feed's ordering.
    public Task<IReadOnlyList<Event>> GetUpcomingByUniversityAsync(
        Guid universityId,
        DateTimeOffset from,
        int limit,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(Event @event) => _db.Events.Add(@event);

    public void Remove(Event @event) => _db.Events.Remove(@event);
}
