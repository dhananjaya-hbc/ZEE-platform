using Zee.Application.Posts.Commands.CreatePost;
using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Application.UnitTests.Posts;

/// <summary>Tests for <see cref="CreatePostCommandValidator"/>.</summary>
///
/// To pick this up: add the rules listed in the validator's TODO, then remove the Skips.
public sealed class CreatePostCommandValidatorTests
{
    private const string Todo = "TODO: implement CreatePostCommandValidator rules, then remove this Skip.";

    private readonly CreatePostCommandValidator _validator = new();

    [Fact(Skip = Todo)]
    public void Accepts_a_well_formed_global_post()
    {
        var result = _validator.Validate(new CreatePostCommand("hello campus"));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(Skip = Todo)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_blank_content(string content)
    {
        _validator.Validate(new CreatePostCommand(content)).IsValid.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Rejects_content_over_the_maximum_length()
    {
        var command = new CreatePostCommand(new string('a', Post.MaxContentLength + 1));

        _validator.Validate(command).IsValid.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Rejects_group_visibility_without_a_group_id()
    {
        var command = new CreatePostCommand("hi", PostVisibility.Group, GroupId: null);

        _validator.Validate(command).IsValid.ShouldBeFalse();
    }

    [Fact(Skip = Todo)]
    public void Rejects_a_group_id_without_group_visibility()
    {
        var command = new CreatePostCommand("hi", PostVisibility.Global, Guid.CreateVersion7());

        _validator.Validate(command).IsValid.ShouldBeFalse();
    }

    /// <summary>
    /// Error messages surface directly in the web UI beneath the field, so they must be
    /// keyed to the property that failed.
    /// </summary>
    [Fact(Skip = Todo)]
    public void Reports_failures_against_the_property_name()
    {
        var result = _validator.Validate(new CreatePostCommand(""));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreatePostCommand.Content));
    }
}
