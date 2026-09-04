using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="User"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("users"). Key: Id.
///   - Email: required, max 254, with a UNIQUE index. The uniqueness constraint has to
///     exist in the database, not just in a handler's "does this email exist" check -
///     two concurrent signups can both pass that check before either commits.
///   - Name: required, max 100. Major: optional, max 100. Year: optional int.
///   - Courses / Interests map to the private _courses and _interests backing fields as
///     text[], via PropertyAccessMode.Field.
///   - Relationship: many Users to one University, HasForeignKey(u => u.UniversityId),
///     OnDelete(DeleteBehavior.Restrict). Deleting a university must not silently cascade
///     away every student on that campus.
///   - Index on UniversityId - campus-scoped queries filter on it constantly.
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
