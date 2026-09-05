using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zee.Domain.Entities;

namespace Zee.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for <see cref="User"/>.</summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        // The database-level guarantee behind IUserRepository.ExistsByEmailAsync - that
        // check alone cannot stop two concurrent sign-ups both passing it before either
        // commits.
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Major)
            .HasMaxLength(100);

        builder.Property(u => u.Courses)
            .HasField("_courses")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("courses")
            .HasColumnType("text[]");

        builder.Property(u => u.Interests)
            .HasField("_interests")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("interests")
            .HasColumnType("text[]");

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.HasOne(u => u.University)
            .WithMany()
            .HasForeignKey(u => u.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Campus-scoped queries (the feed's University-visibility filter, the events
        // calendar, etc.) all filter on this column.
        builder.HasIndex(u => u.UniversityId);
    }
}
