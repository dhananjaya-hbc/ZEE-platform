using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Events.Commands.CreateEvent;
using Zee.Application.Events.Dtos;

namespace Zee.Api.Controllers;

/// <summary>Campus events.</summary>
public sealed class EventsController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Creates an event at the authenticated student's campus.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> Create(
        [FromBody] CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var @event = await Sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = @event.Id }, @event);
    }

    /// <summary>Fetches a single event with its RSVP counts.</summary>
    ///
    /// TODO: Implement. Needs a GetEventByIdQuery + handler.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    /// <summary>Lists upcoming events at the caller's campus, soonest first.</summary>
    ///
    /// TODO: Implement. Needs a GetUpcomingEventsQuery + handler backed by
    /// IEventRepository.GetUpcomingByUniversityAsync.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<IReadOnlyList<EventDto>>> GetUpcoming(CancellationToken cancellationToken)
        => throw new NotImplementedException();

    /// <summary>Records or updates the caller's RSVP.</summary>
    ///
    /// TODO: Implement. Needs a RespondToEventCommand + handler calling Event.Respond.
    /// Load the event with GetByIdWithRsvpsAsync - Respond needs the collection to enforce
    /// one response per student.
    [HttpPut("{id:guid}/rsvp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Rsvp(Guid id, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
