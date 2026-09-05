using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Rsvp"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("rsvps"). Key: Id.
///   - Status: stored as int, required.
///   - UNIQUE index on (EventId, UserId) - the database-level guarantee behind
///     Event.Respond's one-response-per-student rule.
///   - Event and User: many-to-one, OnDelete(DeleteBehavior.Cascade) on both.
public sealed class RsvpConfiguration : IEntityTypeConfiguration<Rsvp>
{
    public void Configure(EntityTypeBuilder<Rsvp> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
