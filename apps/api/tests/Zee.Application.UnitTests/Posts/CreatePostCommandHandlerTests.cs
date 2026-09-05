using Zee.Application.Common.Interfaces;
using Zee.Application.Posts.Commands.CreatePost;
using Zee.Domain.Repositories;

namespace Zee.Application.UnitTests.Posts;

/// <summary>
/// Tests for <see cref="CreatePostCommandHandler"/>.
/// </summary>
/// <remarks>
/// This is the template for testing any handler, and it shows why the layering pays off:
/// there is no database, no HTTP, and no container here. The handler's collaborators are
/// all interfaces from Domain and Application, so NSubstitute can stand in for every one of
/// them and each test runs in microseconds.
///
/// <para>To pick this up: implement CreatePostCommandHandler, then remove the Skips.</para>
/// </remarks>
public sealed class CreatePostCommandHandlerTests
{
    private const string Todo = "TODO: implement CreatePostCommandHandler, then remove this Skip.";

    private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private CreatePostCommandHandler CreateHandler() => new(_posts, _unitOfWork, _currentUser);

    [Fact(Skip = Todo)]
    public async Task Handle_stages_the_post_and_commits_once()
    {
        _currentUser.UserId.Returns(Guid.CreateVersion7());
        _currentUser.UniversityId.Returns(Guid.CreateVersion7());

        await CreateHandler().Handle(new CreatePostCommand("hello campus"), CancellationToken.None);

        _posts.Received(1).Add(Arg.Any<Zee.Domain.Entities.Post>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The author must come from the authenticated principal. If this ever regresses, one
    /// student can post as another - so it is asserted explicitly rather than assumed.
    /// </summary>
    [Fact(Skip = Todo)]
    public async Task Handle_takes_the_author_from_the_current_user()
    {
        var callerId = Guid.CreateVersion7();
        _currentUser.UserId.Returns(callerId);
        _currentUser.UniversityId.Returns(Guid.CreateVersion7());

        var result = await CreateHandler()
            .Handle(new CreatePostCommand("hello"), CancellationToken.None);

        result.AuthorId.ShouldBe(callerId);
    }

    [Fact(Skip = Todo)]
    public async Task Handle_throws_when_the_caller_is_anonymous()
    {
        _currentUser.UserId.Returns((Guid?)null);

        await Should.ThrowAsync<Zee.Application.Common.Exceptions.ForbiddenAccessException>(
            () => CreateHandler().Handle(new CreatePostCommand("hello"), CancellationToken.None));
    }

    [Fact(Skip = Todo)]
    public async Task Handle_does_not_commit_when_the_entity_rejects_the_input()
    {
        _currentUser.UserId.Returns(Guid.CreateVersion7());
        _currentUser.UniversityId.Returns(Guid.CreateVersion7());

        await Should.ThrowAsync<Zee.Domain.Common.DomainException>(
            () => CreateHandler().Handle(new CreatePostCommand("   "), CancellationToken.None));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <remarks>
    /// Requires the IGroupRepository mentioned in the handler's TODO. Posting into a group
    /// you have not joined must be refused - [Authorize] does not cover this.
    /// </remarks>
    [Fact(Skip = "TODO: needs IGroupRepository; see the TODO on CreatePostCommandHandler.")]
    public void Handle_throws_when_posting_to_a_group_the_caller_has_not_joined()
    {
    }
}
