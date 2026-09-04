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
                "Connection string 'Postgres' is not configured. Copy .env.example to .env.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Transient network faults are normal between containers, especially during
                // startup when Postgres may still be accepting its first connections.
                npgsql.EnableRetryOnFailure(maxRetryCount: 3, TimeSpan.FromSeconds(5), null);
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
