using MeAjudaAi.Modules.Locations.API.Endpoints;
using MeAjudaAi.Modules.Locations.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddLocationsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseLocationsModule(this IEndpointRouteBuilder app)
    {
        LocationsEndpoints.Map(app);
        return app;
    }
}

