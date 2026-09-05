using MediatR;
using Zee.Application.Posts.Dtos;
using Zee.Domain.Enums;

namespace Zee.Application.Posts.Commands.CreatePost;

/// <summary>
/// Publishes a new post as the currently authenticated student.
/// </summary>
/// <param name="Content">Body text. Required, at most 5000 characters.</param>
/// <param name="Visibility">Who can see it. Defaults to Global.</param>
/// <param name="GroupId">
/// The group to post into. Required when <paramref name="Visibility"/> is
/// <see cref="PostVisibility.Group"/>, and must be null otherwise.
/// </param>
/// <remarks>
/// <b>There is no AuthorId parameter, and that is deliberate.</b> The author is taken from
/// <c>ICurrentUser</c> inside the handler. If the client could supply it, the endpoint would
/// let any authenticated student post as any other, and the only thing preventing that
/// would be a controller remembering to overwrite the field.
///
/// <para>Commands are records so they are value-comparable and immutable - a handler cannot
/// mutate its own input, and tests can assert on equality.</para>
/// </remarks>
public sealed record CreatePostCommand(
    string Content,
    PostVisibility Visibility = PostVisibility.Global,
    Guid? GroupId = null) : IRequest<PostDto>;
