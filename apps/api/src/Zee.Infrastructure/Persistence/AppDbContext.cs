using Microsoft.EntityFrameworkCore;
using Zee.Domain.Entities;
using Zee.Domain.Repositories;

namespace Zee.Infrastructure.Persistence;

/// <summary>
/// The EF Core context for ZEE's PostgreSQL database.
/// </summary>
/// <remarks>
/// Also implements <see cref="IUnitOfWork"/>. The change tracker already <i>is</i> a unit of
/// work; adding a wrapper class around it would be indirection for its own sake. Handlers
/// still only ever see the interface, so nothing above Infrastructure knows EF Core is what
/// is committing.
/// </remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<University> Universities => Set<University>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();

    public DbSet<Competition> Competitions => Set<Competition>();

    public DbSet<Achievement> Achievements => Set<Achievement>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Rsvp> Rsvps => Set<Rsvp>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // pgvector is enabled now so Phase 2 can add embedding columns without a migration
        // scramble. Nothing in Phase 1 uses it, and an unused extension costs nothing.
        modelBuilder.HasPostgresExtension("vector");

        // Picks up every IEntityTypeConfiguration<T> in this assembly, so adding an entity
        // means adding one configuration file - never editing this method.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
