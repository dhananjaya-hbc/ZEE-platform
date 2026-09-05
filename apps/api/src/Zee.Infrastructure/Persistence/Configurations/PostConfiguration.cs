using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Post"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("posts"). Key: Id.
///   - Content: required, max Post.MaxContentLength.
///   - Visibility: stored as int (the enum's underlying value), required.
///   - Author: many-to-one on AuthorId, OnDelete(DeleteBehavior.Cascade) - deleting an
///     account should take its posts with it.
///   - Group: many-to-one on GroupId, optional, OnDelete(DeleteBehavior.Cascade).
///   - Comments: one-to-many, mapped to the private _comments backing field.
///
///   - INDEX THAT MATTERS: a composite index on (CreatedAt DESC, Id DESC).
///     This is the index the entire chronological feed rides on. Keyset pagination seeks
///     into it directly; without it, every page of every feed is a full sort of the posts
///     table. Include Id, not just CreatedAt - the Id is what makes the ordering total, and
///     an index that stops at CreatedAt cannot serve the tie-break.
///   - Secondary index on (GroupId, CreatedAt DESC) for group feeds.
public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
