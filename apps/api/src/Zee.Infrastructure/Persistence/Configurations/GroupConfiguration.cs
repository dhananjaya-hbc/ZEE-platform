using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Group"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("groups"). Key: Id.
///   - Name: required, max 120. Description: optional, max 1000.
///   - Type: stored as int, required.
///   - University: many-to-one on UniversityId, OPTIONAL (null for GlobalInterest groups),
///     OnDelete(DeleteBehavior.Cascade).
///   - Members: one-to-many, mapped to the private _members backing field via
///     PropertyAccessMode.Field.
///   - Index on (UniversityId, Type) for "clubs at my university" style browsing.
public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
