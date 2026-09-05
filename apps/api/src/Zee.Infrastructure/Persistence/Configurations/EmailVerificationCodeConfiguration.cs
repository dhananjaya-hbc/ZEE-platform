using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="EmailVerificationCode"/>.</summary>
public sealed class EmailVerificationCodeConfiguration : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> builder)
    {
        builder.ToTable("email_verification_codes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(c => c.CodeHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ExpiresAt)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.AttemptCount)
            .IsRequired();

        // "Newest live code for this address" is the only read path, and it runs on
        // every verification attempt.
        builder.HasIndex(c => new { c.Email, c.CreatedAt });

        // Backs the background purge job that deletes spent codes.
        builder.HasIndex(c => c.ExpiresAt);

        builder.HasOne<Zee.Domain.Entities.University>()
            .WithMany()
            .HasForeignKey(c => c.UniversityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
