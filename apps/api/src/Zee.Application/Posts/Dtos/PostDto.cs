using Zee.Domain.Entities;
using Zee.Domain.Enums;

namespace Zee.Application.Posts.Dtos;

/// <summary>What the API returns for a post.</summary>
/// <remarks>
/// Domain entities are never serialised directly. Three reasons, in order of how much
/// trouble they cause:
///
/// <list type="number">
/// <item><description>An entity's shape is driven by persistence and invariants; a
/// response's shape is driven by what a screen needs. Fusing them means every database
/// column becomes public API and every rename is a breaking change.</description></item>
/// <item><description>Serialising an entity walks its navigation properties, which either
/// triggers unintended queries or leaks whatever happens to be loaded.</description></item>
/// <item><description>A DTO is an explicit allowlist. Adding a field to an entity cannot
/// accidentally publish it.</description></item>
/// </list>
/// </remarks>
public sealed record PostDto(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string Content,
    Guid? GroupId,
    string? GroupName,
    PostVisibility Visibility,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt)
{
    /// <summary>Projects an entity to its DTO.</summary>
    ///
    /// TODO: Implement.
    /// Acceptance criteria:
    ///   - Author and Group are navigation properties, populated only when the query
    ///     included them. Fall back to a placeholder ("Unknown") and null rather than
    ///     dereferencing - a missing display name must not take down a whole feed request.
    ///   - Never add fields here that the entity does not already expose publicly.
    public static PostDto FromEntity(Post post)
        => throw new NotImplementedException();
}
