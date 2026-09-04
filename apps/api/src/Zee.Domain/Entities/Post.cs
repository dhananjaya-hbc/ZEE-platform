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
    public static Post Create(
        Guid authorId,
        string content,
        PostVisibility visibility = PostVisibility.Global,
        Guid? groupId = null)
    {
        Guard.NotEmpty(authorId);
        Guard.DefinedEnum(visibility);

        // Group scoping and visibility are two sides of the same fact, so they are kept in
        // lockstep rather than allowed to drift into a state where a post claims group
        // visibility but has no group to be visible to.
        if (visibility == PostVisibility.Group && groupId is null)
        {
            throw new DomainException("A post with Group visibility must specify a groupId.");
        }

        if (visibility != PostVisibility.Group && groupId is not null)
        {
            throw new DomainException("A post with a groupId must use Group visibility.");
        }

        if (groupId is not null)
        {
            Guard.NotEmpty(groupId.Value, nameof(groupId));
        }

        return new Post(
            NewId(),
            authorId,
            Guard.NotEmptyAndAtMost(content, MaxContentLength),
            groupId,
            visibility);
    }

    /// <summary>
    /// Replaces the body and stamps <see cref="EditedAt"/>. Visibility and group are fixed
    /// after creation - changing who can see something already published is a different
    /// operation with different consequences, not an edit.
    /// </summary>
    public void Edit(string content)
    {
        Content = Guard.NotEmptyAndAtMost(content, MaxContentLength);
        EditedAt = DateTimeOffset.UtcNow;
    }
}
