using MeAjudaAi.Modules.Ratings.API.Endpoints;
using MeAjudaAi.Modules.Ratings.Application;
using MeAjudaAi.Modules.Ratings.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços do módulo Ratings (Application + Infrastructure).
    /// </summary>
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Ratings no pipeline de rotas.
    /// </summary>
    public static IEndpointRouteBuilder UseRatingsModule(this IEndpointRouteBuilder app)
    {
        RatingsEndpoints.Map(app);
        return app;
    }
}