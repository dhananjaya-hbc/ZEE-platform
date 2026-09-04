using MediatR;
using Zee.Application.Common.Interfaces;
using Zee.Application.Events.Dtos;
using Zee.Domain.Repositories;

namespace Zee.Application.Events.Commands.CreateEvent;

/// <summary>Handles <see cref="CreateEventCommand"/>.</summary>
///
/// TODO: Implement Handle.
/// Acceptance criteria:
///   1. Read userId and universityId from _currentUser; throw ForbiddenAccessException if
///      either is null.
///   2. Build with Event.Create(title, universityId, location, start, end, createdBy: userId,
///      description). Note universityId comes from the CALLER, never from the request.
///   3. _events.Add(@event), then await _unitOfWork.SaveChangesAsync(cancellationToken).
///   4. Return EventDto.FromEntity(@event, viewerId: userId). A newly created event has no
///      RSVPs, so the counts are 0 and ViewerStatus is null.
///
/// Tests in tests/Zee.Application.UnitTests/Events/CreateEventCommandHandlerTests.cs.
public sealed class CreateEventCommandHandler(
    IEventRepository events,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IEventRepository _events = events;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
