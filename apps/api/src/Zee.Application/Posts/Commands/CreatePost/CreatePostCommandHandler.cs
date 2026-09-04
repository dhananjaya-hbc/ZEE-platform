using MediatR;
using Zee.Application.Common.Interfaces;
using Zee.Application.Posts.Dtos;
using Zee.Domain.Repositories;

namespace Zee.Application.Posts.Commands.CreatePost;

/// <summary>
/// Handles <see cref="CreatePostCommand"/>.
/// </summary>
/// <remarks>
/// The shape of this handler is the shape every other one should copy, so it is worth
/// noticing what is <i>absent</i> from it:
///
/// <list type="bullet">
/// <item><description><b>No validation.</b> <c>ValidationBehaviour</c> has already run the
/// validator by the time this executes.</description></item>
/// <item><description><b>No DbContext.</b> It depends on <c>IPostRepository</c> and
/// <c>IUnitOfWork</c>, both declared in Domain, so this class has no idea EF Core exists
/// and can be unit tested with substitutes.</description></item>
/// <item><description><b>No try/catch.</b> The API's exception middleware maps
/// <c>DomainException</c>, <c>NotFoundException</c> and friends to status codes in one
/// place.</description></item>
/// <item><description><b>No author parameter.</b> Identity comes from
/// <c>ICurrentUser</c>, never from the request body.</description></item>
/// </list>
/// </remarks>
///
/// TODO: Implement Handle.
/// Acceptance criteria:
///   1. Read the author id from _currentUser.UserId. If null, throw
///      ForbiddenAccessException - the endpoint is [Authorize]d, so a null here means the
///      pipeline is misconfigured and failing closed is the only safe response.
///   2. If command.GroupId is set, load the group and verify the author is a member.
///      Throw ForbiddenAccessException if not. Skipping this check would let anyone post
///      into any group by supplying its id, which the [Authorize] attribute does NOT cover.
///      (Needs an IGroupRepository - add one alongside the existing repository interfaces.)
///   3. Build the entity with Post.Create(...). Let it throw; do not pre-validate.
///   4. _posts.Add(post), then await _unitOfWork.SaveChangesAsync(cancellationToken).
///      The repository only stages the change - nothing is written until the unit of work
///      commits.
///   5. Return PostDto.FromEntity(post).
///
/// Tests in tests/Zee.Application.UnitTests/Posts/CreatePostCommandHandlerTests.cs.
/// Cover at minimum: the happy path, anonymous caller, and non-member posting to a group.
public sealed class CreatePostCommandHandler(
    IPostRepository posts,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _posts = posts;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUser _currentUser = currentUser;

    public Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
