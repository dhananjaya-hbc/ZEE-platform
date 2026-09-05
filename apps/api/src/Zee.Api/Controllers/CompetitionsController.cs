using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Competitions.Commands.CreateCompetition;
using Zee.Application.Competitions.Dtos;

namespace Zee.Api.Controllers;

/// <summary>Competition listings and team recruiting.</summary>
public sealed class CompetitionsController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Lists a new competition organised by the authenticated student.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompetitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompetitionDto>> Create(
        [FromBody] CreateCompetitionCommand command,
        CancellationToken cancellationToken)
    {
        var competition = await Sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = competition.Id }, competition);
    }

    /// <summary>Fetches a single competition.</summary>
    ///
    /// TODO: Implement. Needs a GetCompetitionByIdQuery + handler.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompetitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<CompetitionDto>> GetById(Guid id, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    /// <summary>Browses competitions open to the caller.</summary>
    ///
    /// TODO: Implement. Needs a BrowseCompetitionsQuery + handler backed by
    /// ICompetitionRepository.BrowseAsync. Remember the scope filter returns listings open
    /// to all campuses PLUS the caller's own - never another university's private ones.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompetitionDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<IReadOnlyList<CompetitionDto>>> Browse(CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
