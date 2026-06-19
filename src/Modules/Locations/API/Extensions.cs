using MeAjudaAi.Modules.Locations.API.Endpoints;
using MeAjudaAi.Modules.Locations.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.API;

/// <summary>
/// Métodos de extensão para registrar serviços e endpoints do módulo Locations.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Locations.
    /// </summary>
    public static IServiceCollection AddLocationsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Locations.
    /// </summary>
    public static WebApplication UseLocationsModule(this WebApplication app)
    {
        app.MapLocationsEndpoints();

        return app;
    }

    public static IEndpointRouteBuilder MapLocationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        LocationsEndpoints.Map(endpoints);
        return endpoints;
    }
}