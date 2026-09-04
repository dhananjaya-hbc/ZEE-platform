using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zee.Application.Common.Interfaces;
using Zee.Domain.Repositories;
using Zee.Infrastructure.Ai;
using Zee.Infrastructure.Persistence;
using Zee.Infrastructure.Persistence.Repositories;

namespace Zee.Infrastructure;

/// <summary>Registers everything the Infrastructure layer provides.</summary>
/// <remarks>
/// This is the only place in the codebase where an interface from Domain or Application is
/// bound to a concrete implementation. That is what makes the dependency rule real rather
/// than aspirational: swapping PostgreSQL for something else, or the HTTP AI client for an
/// in-process one, is a change to this file and nothing above it.
/// </remarks>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        AddPersistence(services, configuration);
        AddCaching(services, configuration);
        AddAiService(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured. ZEE uses Neon (serverless "
                + "PostgreSQL) - create a free project at https://neon.com, then copy "
                + "infra/.env.example to infra/.env and set DATABASE_URL. "
                + "See docs/Database.md.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Tuned for Neon rather than a local container.
                //
                // Neon scales compute to zero after a few minutes idle, so the first query
                // after a quiet period pays a resume - usually well under a second, but it
                // can surface as a transient connection failure rather than slow success.
                // Retrying is the difference between that being invisible and it being a
                // 500 for whoever happened to make the first request of the morning.
                //
                // More attempts and a longer ceiling than a local database would need,
                // because the failure being absorbed here is a cold start, not a crash.
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                // Neon is remote, so every query crosses a network. The default 30s is
                // long enough that a hung query holds a request open well past the point
                // the caller has given up.
                npgsql.CommandTimeout(30);
            }));

        // The DbContext IS the unit of work - its change tracker already accumulates the
        // pending writes - so it is resolved rather than wrapped in another class.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUniversityRepository, UniversityRepository>();
        services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redis = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redis))
        {
            // Falling back to an in-memory cache keeps `dotnet run` working with no Redis
            // container, which matters for anyone contributing a frontend or docs change.
            services.AddDistributedMemoryCache();
            return;
        }

        services.AddStackExchangeRedisCache(options => options.Configuration = redis);
    }

    private static void AddAiService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiServiceOptions>()
            .Bind(configuration.GetSection(AiServiceOptions.SectionName))
            .ValidateDataAnnotations()
            // Validate at startup, not on first use. A missing internal key should stop the
            // container coming up, not surface as a confusing 401 hours later.
            .ValidateOnStart();

        services.AddHttpClient<IAiServiceClient, AiServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<AiServiceOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Set once here so no individual call can forget it.
            client.DefaultRequestHeaders.Add("X-Internal-Key", options.InternalKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}
