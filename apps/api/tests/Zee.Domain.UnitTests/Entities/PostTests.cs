using Zee.Domain.Common;
using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Domain.UnitTests.Entities;

/// <summary>
/// Invariants of <see cref="Post"/>.
/// </summary>
/// <remarks>
/// Every test here is marked Skip because Post.Create is still a stub - see the TODO on it.
/// CI must stay green on a fresh clone, otherwise a contributor cannot tell their own change
/// from the scaffolding's existing failures.
///
/// <para><b>To pick this up:</b> implement Post.Create, then delete the Skip argument from
/// each Fact and Theory below and make them pass. The test names are the specification.</para>
/// </remarks>
public sealed class PostTests
{
    private static readonly Guid AuthorId = Guid.CreateVersion7();
    private static readonly Guid GroupId = Guid.CreateVersion7();

    private const string Todo = "TODO: implement Post.Create, then remove this Skip.";

    [Theory(Skip = Todo)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n  ")]
    public void Create_throws_when_content_is_blank(string content)
    {
        Should.Throw<DomainException>(() => Post.Create(AuthorId, content));
    }

    [Fact(Skip = Todo)]
    public void Create_throws_when_content_exceeds_the_maximum_length()
    {
        var tooLong = new string('a', Post.MaxContentLength + 1);

        Should.Throw<DomainException>(() => Post.Create(AuthorId, tooLong));
    }

    [Fact(Skip = Todo)]
    public void Create_trims_surrounding_whitespace_from_content()
    {
        var post = Post.Create(AuthorId, "  hello campus  ");

        post.Content.ShouldBe("hello campus");
    }

    [Fact(Skip = Todo)]
    public void Create_throws_when_the_author_id_is_empty()
    {
        Should.Throw<DomainException>(() => Post.Create(Guid.Empty, "valid content"));
    }

    [Fact(Skip = Todo)]
    public void Create_throws_when_group_visibility_has_no_group()
    {
        Should.Throw<DomainException>(
            () => Post.Create(AuthorId, "content", PostVisibility.Group, groupId: null));
    }

    [Theory(Skip = Todo)]
    [InlineData(PostVisibility.Global)]
    [InlineData(PostVisibility.University)]
    public void Create_throws_when_a_group_is_given_without_group_visibility(PostVisibility visibility)
    {
        Should.Throw<DomainException>(
            () => Post.Create(AuthorId, "content", visibility, GroupId));
    }

    [Fact(Skip = Todo)]
    public void Create_accepts_a_group_post_with_matching_visibility()
    {
        var post = Post.Create(AuthorId, "content", PostVisibility.Group, GroupId);

        post.GroupId.ShouldBe(GroupId);
        post.Visibility.ShouldBe(PostVisibility.Group);
    }

    [Fact(Skip = Todo)]
    public void Create_defaults_to_global_visibility_and_leaves_EditedAt_null()
    {
        var post = Post.Create(AuthorId, "content");

        post.Visibility.ShouldBe(PostVisibility.Global);
        post.EditedAt.ShouldBeNull();
        post.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact(Skip = Todo)]
    public void Edit_replaces_the_content_and_stamps_EditedAt()
    {
        var post = Post.Create(AuthorId, "original");

        post.Edit("revised");

        post.Content.ShouldBe("revised");
        post.EditedAt.ShouldNotBeNull();
    }

    [Fact(Skip = Todo)]
    public void Edit_throws_when_the_new_content_is_blank()
    {
        var post = Post.Create(AuthorId, "original");

        Should.Throw<DomainException>(() => post.Edit("   "));
        post.Content.ShouldBe("original");
    }
}
