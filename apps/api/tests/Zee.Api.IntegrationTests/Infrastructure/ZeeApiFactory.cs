using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zee.Infrastructure.Persistence;

namespace Zee.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API in memory, with PostgreSQL swapped for the EF Core in-memory provider.
/// </summary>
/// <remarks>
/// Everything else is genuine: routing, model binding, the exception middleware, MediatR
/// dispatch, DI. That is what these tests are for - the wiring between layers, which unit
/// tests cannot see.
///
/// <para><b>Known limitation.</b> The in-memory provider is not a relational database. It
/// ignores unique indexes and foreign keys, and it cannot tell you whether a LINQ query
/// translates to SQL. So it is right for "does the pipeline work" and wrong for "is this
/// query correct" - the unique constraints on (EventId, UserId) and users.email, in
/// particular, are simply not enforced here.</para>
///
/// <para>The intended upgrade is Testcontainers with a real pgvector Postgres image. It was
/// left out of Phase 1 so CI needs no Docker daemon and a first-time contributor can run
/// <c>dotnet test</c> immediately. See docs/Architecture.md.</para>
/// </remarks>
public sealed class ZeeApiFactory : WebApplicationFactory<Program>
{
    /// <summary>Signing key used only by the test host. Not a secret; never used anywhere real.</summary>
    public const string TestSigningKey = "test-only-signing-key-at-least-32-characters-long";

    /// <summary>Issuer and audience the test host validates against.</summary>
    public const string TestIssuer = "zee";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // "Testing" rather than "Development": Program.cs runs Database.MigrateAsync() on
        // startup in Development, and migrations cannot be applied to an in-memory provider.
        builder.UseEnvironment("Testing");

        // The host validates its configuration at startup and refuses to boot without a JWT
        // signing key, a Postgres connection string and the AI service settings. That check
        // is deliberate - it is what stops a misconfigured container running with unsigned
        // tokens - so the test host has to satisfy it rather than have it relaxed.
        //
        // These values are fake and never leave the test process: the DbContext is replaced
        // below before the connection string is used, and no test reaches the AI service.
        // UseSetting, not ConfigureAppConfiguration. Program.cs reads these values while
        // building the host, which happens BEFORE ConfigureAppConfiguration callbacks run -
        // so configuring them that way leaves the startup checks looking at nothing.
        builder.UseSetting("Jwt:Key", TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestIssuer);
        builder.UseSetting(
            "ConnectionStrings:Postgres",
            "Host=localhost;Database=zee-tests;Username=zee;Password=zee");
        builder.UseSetting("AiService:BaseUrl", "http://ai-service.invalid");
        builder.UseSetting("AiService:InternalKey", "test-internal-key-not-a-real-secret");

        builder.ConfigureServices(services =>
        {
            RemoveNpgsqlRegistrations(services);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"zee-tests-{Guid.CreateVersion7()}"));
        });
    }

    /// <summary>
    /// Strips every trace of the Npgsql provider so the in-memory one can take its place.
    /// </summary>
    /// <remarks>
    /// Removing <c>DbContextOptions&lt;AppDbContext&gt;</c> alone is not enough, and the
    /// failure is confusing when you get it wrong: <c>AddDbContext</c> also registers the
    /// provider's own internal services and, on EF Core 9+, an
    /// <c>IDbContextOptionsConfiguration&lt;AppDbContext&gt;</c> descriptor that re-applies
    /// <c>UseNpgsql</c>. Leave either behind and EF finds two providers registered and
    /// throws at first use - which surfaces as an unhealthy health check rather than
    /// anything pointing at configuration.
    /// </remarks>
    private static void RemoveNpgsqlRegistrations(IServiceCollection services)
    {
        var doomed = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                || d.ServiceType == typeof(DbContextOptions)
                || d.ServiceType == typeof(AppDbContext)
                || d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal) == true
                || d.ServiceType.Namespace?.StartsWith("Npgsql", StringComparison.Ordinal) == true)
            .ToList();

        foreach (var descriptor in doomed)
        {
            services.Remove(descriptor);
        }
    }

    /// TODO: Add a helper that issues a valid JWT for a fake student, so tests can exercise
    /// authenticated endpoints. Without it, only anonymous routes such as /health are
    /// reachable. Sign it with the same Jwt:Key the test host is configured with, and
    /// include the NameIdentifier and zee:university_id claims CurrentUser reads.
    public string CreateAccessTokenFor(Guid userId, Guid universityId)
        => throw new NotImplementedException();
}
