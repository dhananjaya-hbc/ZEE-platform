using Zee.Domain.Entities;

namespace Zee.Application.Events.Dtos;

/// <summary>What the API returns for a campus event.</summary>
/// <param name="GoingCount">Number of students who responded Going.</param>
/// <param name="InterestedCount">Number who responded Interested.</param>
/// <param name="ViewerStatus">
/// The requesting student's own response as a string, or null if they have not responded.
/// </param>
/// <remarks>
/// The counts are here rather than as a separate endpoint because every event card in the
/// UI shows them - splitting them out would turn one list request into N+1 round trips.
/// </remarks>
public sealed record EventDto(
    Guid Id,
    string Title,
    Guid UniversityId,
    string Location,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Description,
    Guid CreatedBy,
    string CreatorName,
    int GoingCount,
    int InterestedCount,
    string? ViewerStatus)
{
    /// <summary>Projects an entity to its DTO.</summary>
    /// <param name="viewerId">The requesting student, used to fill <see cref="ViewerStatus"/>.</param>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Counts come from the Rsvps collection, so the caller must have loaded it via
    ///     IEventRepository.GetByIdWithRsvpsAsync. When it is empty, report 0 rather than
    ///     throwing - an un-included navigation is a caller mistake, not a 500 for the user.
    ///   - ViewerStatus is null when the viewer has no RSVP.
    ///   - CreatorName falls back to "Unknown" when the Creator navigation is not loaded.
    public static EventDto FromEntity(Event @event, Guid viewerId)
        => throw new NotImplementedException();
}
