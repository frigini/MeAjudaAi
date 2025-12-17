using MeAjudaAi.Modules.Locations.API.Endpoints;
using MeAjudaAi.Modules.Locations.Application;
using MeAjudaAi.Modules.Locations.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.Locations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Locations.API;

/// <summary>
/// Métodos de extensão para registrar serviços e endpoints do módulo Locations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Locations.
    /// </summary>
    public static IServiceCollection AddLocationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Locations.
    /// Registra endpoints administrativos para gerenciamento de cidades permitidas.
    /// </summary>
    public static WebApplication UseLocationsModule(this WebApplication app)
    {
        app.MapLocationsEndpoints();

        return app;
    }

    /// <summary>
    /// Mapeia os endpoints do módulo Locations.
    /// </summary>
    private static IEndpointRouteBuilder MapLocationsEndpoints(this IEndpointRouteBuilder app)
    {
        // Registrar endpoints administrativos (Admin only)
        CreateAllowedCityEndpoint.Map(app);
        GetAllAllowedCitiesEndpoint.Map(app);
        GetAllowedCityByIdEndpoint.Map(app);
        UpdateAllowedCityEndpoint.Map(app);
        DeleteAllowedCityEndpoint.Map(app);

        return app;
    }
}
