using Zee.Domain.Common;
using Zee.Domain.Enums;

namespace Zee.Domain.Entities;

/// <summary>
/// A student's post: the unit the feed is built from.
/// </summary>
/// <remarks>
/// <b>Phase 1 has no ranking.</b> The feed is ordered by <see cref="CreatedAt"/> descending
/// and nothing else. There is deliberately no score, weight or engagement column here -
/// Phase 2 computes ranking in the AI service and returns post ids with scores, leaving
/// this entity unchanged.
/// </remarks>
public sealed class Post : Entity
{
    /// <summary>Longest body a post may have. Enforced here and mirrored by the EF Core config.</summary>
    public const int MaxContentLength = 5000;

    private readonly List<Comment> _comments = [];

    /// <summary>Required by EF Core.</summary>
    private Post()
    {
    }

    private Post(Guid id, Guid authorId, string content, Guid? groupId, PostVisibility visibility)
        : base(id)
    {
        AuthorId = authorId;
        Content = content;
        GroupId = groupId;
        Visibility = visibility;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>The student who wrote it. Immutable.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Body text.</summary>
    public string Content { get; private set; } = null!;

    /// <summary>The group this was posted to, or null for a personal post.</summary>
    public Guid? GroupId { get; private set; }

    /// <summary>Who can see it.</summary>
    public PostVisibility Visibility { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Set the first time the body is edited; null means never edited.</summary>
    public DateTimeOffset? EditedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Author { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public Group? Group { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    /// <summary>
    /// Creates a post.
    /// </summary>
    /// <exception cref="DomainException">
    /// If the content is empty or whitespace, exceeds <see cref="MaxContentLength"/>,
    /// the author id is empty, or group scoping and visibility disagree.
    /// </exception>
    ///
    /// TODO: Implement. This is the reference example walked through in CONTRIBUTING.md,
    /// so keep it readable.
    ///
    /// Acceptance criteria:
    ///   - Empty or whitespace-only content throws DomainException. (Explicitly required.)
    ///   - Content longer than MaxContentLength throws.
    ///   - authorId of Guid.Empty throws.
    ///   - visibility must be a defined enum member.
    ///   - Visibility == Group REQUIRES groupId; any other visibility REQUIRES groupId null.
    ///     Keep these two facts in lockstep - a post claiming group visibility with no group
    ///     to be visible to has no correct interpretation.
    ///   - CreatedAt is set to UtcNow; EditedAt starts null.
    ///   - Unit tests in tests/Zee.Domain.UnitTests/Entities/PostTests.cs.
    public static Post Create(
        Guid authorId,
        string content,
        PostVisibility visibility = PostVisibility.Global,
        Guid? groupId = null)
        => throw new NotImplementedException();

    /// <summary>
    /// Replaces the body and stamps <see cref="EditedAt"/>.
    /// </summary>
    /// <remarks>
    /// Visibility and group are intentionally not editable. Widening the audience of
    /// something already published is a different operation with different consequences,
    /// not an edit.
    /// </remarks>
    ///
    /// TODO: Implement (same content rules as Create, then set EditedAt to UtcNow).
    public void Edit(string content)
        => throw new NotImplementedException();
}
