using Zee.Domain.Common;

namespace Zee.Domain.Entities;

/// <summary>A reply on a <see cref="Post"/>.</summary>
/// <remarks>
/// Flat, not threaded. Nested replies are a product decision with real UI cost, so Phase 1
/// keeps a single level; adding a nullable <c>ParentCommentId</c> later is an additive
/// migration if that changes.
/// </remarks>
public sealed class Comment : Entity
{
    public const int MaxContentLength = 2000;

    /// <summary>Required by EF Core.</summary>
    private Comment()
    {
    }

    private Comment(Guid id, Guid postId, Guid authorId, string content)
        : base(id)
    {
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>The post being replied to.</summary>
    public Guid PostId { get; private set; }

    /// <summary>The student who wrote the comment.</summary>
    public Guid AuthorId { get; private set; }

    public string Content { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? EditedAt { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public Post? Post { get; private set; }

    /// <summary>Navigation property. Populated only when a query explicitly includes it.</summary>
    public User? Author { get; private set; }

    /// <summary>Creates a comment.</summary>
    /// <exception cref="DomainException">If the content is empty or too long.</exception>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - postId and authorId must not be Guid.Empty.
    ///   - Content required, trimmed, max MaxContentLength.
    public static Comment Create(Guid postId, Guid authorId, string content)
        => throw new NotImplementedException();

    /// <summary>Replaces the body and stamps <see cref="EditedAt"/>.</summary>
    ///
    /// TODO: Implement.
    public void Edit(string content)
        => throw new NotImplementedException();
}
