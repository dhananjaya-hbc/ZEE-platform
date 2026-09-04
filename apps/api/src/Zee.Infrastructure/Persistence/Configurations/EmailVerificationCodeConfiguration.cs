using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="EmailVerificationCode"/>.</summary>
///
/// TODO: Implement Configure. (Empty body is deliberate - see UniversityConfiguration.)
/// Acceptance criteria:
///   - ToTable("email_verification_codes"). Key: Id.
///   - Email: required, max 254. CodeHash: required, max 200.
///   - Index on (Email, CreatedAt DESC) - "newest live code for this address" is the only
///     read path, and it runs on every verification attempt.
///   - Index on ExpiresAt for the background purge job.
///   - University: many-to-one on UniversityId, OnDelete(DeleteBehavior.Cascade).
///   - Do NOT add a unique constraint on Email. Several codes per address legitimately
///     coexist (a student requests a new one before the old expires); "which one counts" is
///     a query-ordering decision, not a schema constraint.
public sealed class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        // Mapping goes here. See the acceptance criteria above.
    }
}
