using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Comment"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("comments"). Key: Id.
///   - Content: required, max Comment.MaxContentLength.
///   - Post: many-to-one on PostId, OnDelete(DeleteBehavior.Cascade).
///   - Author: many-to-one on AuthorId, OnDelete(DeleteBehavior.Cascade).
///   - Index on (PostId, CreatedAt) - comments are always read as "this post's thread,
///     oldest first", never as a global list.
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
