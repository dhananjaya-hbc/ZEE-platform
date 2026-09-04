using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Zee.Application.Common.Behaviours;

namespace Zee.Application;

/// <summary>
/// Registers everything the Application layer provides.
/// </summary>
/// <remarks>
/// Each layer owns its own registration extension, so <c>Program.cs</c> reads as
/// <c>AddApplication().AddInfrastructure(config)</c> rather than a hundred lines of
/// wiring - and, more importantly, so the Api layer never needs to name an internal type
/// from another layer just to register it.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>Adds MediatR, the validation pipeline, and every validator in this assembly.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Assembly scanning is what makes a new feature a single-file addition: drop in a
        // handler or a validator anywhere under this project and it is wired up, with no
        // registration list to remember to update.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Open generic, so it applies to every request type. Registration order is
        // execution order, so anything added before this runs first - keep validation
        // early, in front of handlers that would otherwise act on bad input.
        services.AddTransient(
            typeof(MediatR.IPipelineBehavior<,>),
            typeof(ValidationBehaviour<,>));

        return services;
    }
}
