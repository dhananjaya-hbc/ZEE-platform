using MediatR;
using Zee.Application.Events.Dtos;

namespace Zee.Application.Events.Commands.CreateEvent;

/// <summary>
/// Creates a campus event, hosted at the authenticated student's own university.
/// </summary>
/// <param name="Title">Event name.</param>
/// <param name="Location">Room, building, or a URL for a virtual room.</param>
/// <param name="StartTime">When it begins.</param>
/// <param name="EndTime">When it ends. Must be after <paramref name="StartTime"/>.</param>
/// <param name="Description">Optional detail.</param>
/// <remarks>
/// No <c>UniversityId</c> and no <c>CreatedBy</c>. Both come from <c>ICurrentUser</c> in the
/// handler: a student creates events at their own campus, and letting the request name a
/// different university would let anyone post events onto any campus's calendar.
/// </remarks>
public sealed record CreateEventCommand(
    string Title,
    string Location,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Description = null) : IRequest<EventDto>;
