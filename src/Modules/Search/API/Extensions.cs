using MeAjudaAi.Modules.Search.API.Endpoints;
using MeAjudaAi.Modules.Search.Application;
using MeAjudaAi.Modules.Search.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Search.API;

/// <summary>
/// Module-level extensions for registering the complete Search & Discovery module.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers all Search module services (Domain, Application, Infrastructure, API).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read connection strings and settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSearchModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Domain layer has no external dependencies to register

        // Application layer
        services.AddSearchApplication();

        // Infrastructure layer (requires configuration for DB connection)
        services.AddSearchInfrastructure(configuration);

        // API layer - no additional services to register

        return services;
    }

    /// <summary>
    /// Maps all Search module endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder UseSearchModule(this IEndpointRouteBuilder app)
    {
        SearchProvidersEndpoint.Map(app);
        return app;
    }
}
