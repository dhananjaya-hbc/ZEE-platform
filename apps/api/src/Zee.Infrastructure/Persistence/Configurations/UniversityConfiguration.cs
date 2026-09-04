using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="University"/>.</summary>
///
/// NOTE: every Configure method in this folder is intentionally left EMPTY rather than
/// throwing. OnModelCreating runs them all at startup, so a NotImplementedException here
/// would stop the API booting and block everyone. Empty means EF Core falls back to its
/// conventions: the schema still builds and the app still runs, just without the explicit
/// column types, lengths and indexes listed below.
///
/// TODO: Implement Configure.
/// Acceptance criteria:
///   - ToTable("universities"). Key: Id.
///   - Name: required, max 200. Country: required, fixed length 2.
///   - VerifiedEmailDomains maps to the private _verifiedEmailDomains backing field as a
///     PostgreSQL text[]. Use Metadata.SetPropertyAccessMode(PropertyAccessMode.Field) -
///     the property is read-only (IReadOnlyCollection) so EF cannot set it directly.
///   - Add a GIN index on VerifiedEmailDomains. Every sign-in does a "which university
///     claims this domain" lookup against it; without the index that is a sequential scan
///     over every university on the platform, on the hottest auth path.
///   - IsActive: required, default true.
public sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
