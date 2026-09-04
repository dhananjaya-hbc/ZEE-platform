using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Common.Models;
using Zee.Application.Posts.Dtos;
using Zee.Application.Posts.Queries.GetFeed;

namespace Zee.Api.Controllers;

/// <summary>The authenticated student's feed.</summary>
/// <remarks>
/// <b>Phase 1 serves posts in reverse-chronological order.</b> There is no ranking, and this
/// controller does not call the AI service. When Phase 2 adds ranking it will arrive behind
/// this same route, so clients need no changes.
/// </remarks>
public sealed class FeedController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Returns one page of the feed, newest first.</summary>
    /// <param name="cursor">Opaque cursor from the previous page. Omit for the first page.</param>
    /// <param name="limit">Page size, 1-50.</param>
    /// <response code="200">A page of posts, with a nextCursor when more remain.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CursorPage<PostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CursorPage<PostDto>>> Get(
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetFeedQuery(cursor, limit), cancellationToken));
}
