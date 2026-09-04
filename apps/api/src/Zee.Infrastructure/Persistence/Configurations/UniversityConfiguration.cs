using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="University"/>.</summary>
public sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.ToTable("universities");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Country)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        // Maps directly to the private backing field, since VerifiedEmailDomains itself
        // is a read-only computed property with no setter EF Core could write through.
        builder.Property(u => u.VerifiedEmailDomains)
            .HasField("_verifiedEmailDomains")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("verified_email_domains")
            .HasColumnType("text[]");

        // GIN, not the default B-tree: every sign-in queries "which university claims
        // this domain" against this column, and a GIN index is what makes an array
        // containment lookup fast instead of a sequential scan.
        builder.HasIndex(u => u.VerifiedEmailDomains)
            .HasMethod("gin");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
