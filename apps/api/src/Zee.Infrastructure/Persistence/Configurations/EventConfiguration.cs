using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="Event"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("events"). Key: Id.
///   - Title: required, max 200. Location: required, max 300. Description: optional, max 5000.
///   - University: many-to-one on UniversityId, REQUIRED, OnDelete(DeleteBehavior.Cascade).
///   - Creator: many-to-one on CreatedBy, OnDelete(DeleteBehavior.Restrict).
///   - Rsvps: one-to-many, mapped to the private _rsvps backing field.
///   - Index on (UniversityId, StartTime) - the campus calendar query is "my university,
///     starting after now, soonest first".
public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
