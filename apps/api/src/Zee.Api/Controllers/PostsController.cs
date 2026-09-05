using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zee.Application.Posts.Commands.CreatePost;
using Zee.Application.Posts.Dtos;

namespace Zee.Api.Controllers;

/// <summary>Creating and reading posts.</summary>
/// <remarks>
/// Note how little is here. The controller binds the request, dispatches it, and shapes the
/// HTTP response - no validation, no authorisation logic, no database access. Everything
/// worth testing lives in the handler, which needs no HTTP pipeline to test.
///
/// <para>If you find yourself adding an <c>if</c> to a controller, it probably belongs in a
/// handler or a validator instead.</para>
/// </remarks>
public sealed class PostsController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Publishes a post as the authenticated student.</summary>
    /// <response code="201">The post was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="403">The caller may not post to the requested group.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PostDto>> Create(
        [FromBody] CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var post = await Sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    /// <summary>Fetches a single post.</summary>
    ///
    /// TODO: Implement. Needs a GetPostByIdQuery + handler in the Application layer,
    /// following the GetFeed pattern. It exists now because CreatedAtAction above needs a
    /// route to point at.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PostDto>> GetById(Guid id, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
