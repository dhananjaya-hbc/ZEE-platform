using FluentValidation;
using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Application.Posts.Commands.CreatePost;

/// <summary>
/// Shape validation for <see cref="CreatePostCommand"/>, run automatically by
/// <c>ValidationBehaviour</c> before the handler.
/// </summary>
/// <remarks>
/// This overlaps with <c>Post.Create</c> on purpose, and the duplication is doing real work.
/// The two checks answer different questions and fail differently:
///
/// <list type="bullet">
/// <item><description>The validator answers "is this request well formed", knows the field
/// names, and reports every problem at once as HTTP 400.</description></item>
/// <item><description>The entity answers "is this object legal", holds regardless of how it
/// was constructed - a seeder, a background job, a future gRPC endpoint - and throws.</description></item>
/// </list>
///
/// <para>Dropping the validator would give students a bare 400 with one error. Dropping the
/// entity check would mean any new call path could create an invalid post. Both stay.</para>
/// </remarks>
///
/// WARNING: this validator currently enforces NOTHING. Until the rules below are added,
/// every CreatePostCommand passes straight through to the handler.
///
/// TODO: Implement the rules.
/// Acceptance criteria:
///   - Content: NotEmpty, MaximumLength(Post.MaxContentLength).
///   - Visibility: IsInEnum.
///   - GroupId: NotNull when Visibility == PostVisibility.Group.
///   - GroupId: Null    when Visibility != PostVisibility.Group.
///   - GroupId: NotEqual(Guid.Empty) when it has a value.
///   - Give every rule a .WithMessage() written for a student to read, not a developer -
///     these strings surface directly in the web UI under the field.
///   - Tests in tests/Zee.Application.UnitTests/Posts/CreatePostCommandValidatorTests.cs.
public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        // Rules go here. See the acceptance criteria above.
        // Example of the intended style:
        //
        //     RuleFor(c => c.Content)
        //         .NotEmpty().WithMessage("A post must have some content.")
        //         .MaximumLength(Post.MaxContentLength)
        //         .WithMessage($"A post must be {Post.MaxContentLength} characters or fewer.");
    }
}
