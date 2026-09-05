using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="GroupMembership"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("group_memberships"). Key: Id.
///   - UNIQUE index on (GroupId, UserId). Group.AddMember already de-duplicates in memory,
///     but two concurrent join requests can each load the group, each see no membership,
///     and both insert. Only the database constraint actually prevents that.
///   - Index on UserId alone - "which groups am I in" runs on every feed request to
///     resolve group-visibility posts.
///   - Group and User: many-to-one, OnDelete(DeleteBehavior.Cascade) on both.
public sealed class GroupMembershipConfiguration : IEntityTypeConfiguration<GroupMembership>
{
    public void Configure(EntityTypeBuilder<GroupMembership> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
