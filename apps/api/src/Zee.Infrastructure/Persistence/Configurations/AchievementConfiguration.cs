using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Achievement"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("achievements"). Key: Id.
///   - Title: required, max 200. Description: optional, max 2000.
///   - ProofUrl: optional, max 2048.
///   - User: many-to-one on UserId, OnDelete(DeleteBehavior.Cascade).
///   - Competition: many-to-one on CompetitionId, OPTIONAL,
///     OnDelete(DeleteBehavior.SetNull). Removing a competition listing must not erase the
///     achievements students earned at it - it just unlinks them.
///   - Index on (UserId, Date DESC) for the profile timeline.
public sealed class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
