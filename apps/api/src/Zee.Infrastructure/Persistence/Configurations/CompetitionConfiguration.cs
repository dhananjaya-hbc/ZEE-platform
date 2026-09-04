using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Competition"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("competitions"). Key: Id.
///   - Title: required, max 200. Description: required, max 10000.
///   - Category: stored as int, required.
///   - Organizer: many-to-one on OrganizerId, OnDelete(DeleteBehavior.Restrict) - a
///     competition outlives the account that listed it, and silently deleting other
///     students' listings is worse than leaving an orphan reference.
///   - University: many-to-one on UniversityId, OPTIONAL (null = open to all campuses).
///   - Index on (UniversityId, EndDate) - the browse query filters on campus and
///     "not finished yet" together.
///   - Index on RecruitingTeams for the "find a team" surface.
public sealed class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
