using MeAjudaAi.Modules.Ratings.API.Endpoints;
using MeAjudaAi.Modules.Ratings.Application;
using MeAjudaAi.Modules.Ratings.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseRatingsModule(this IEndpointRouteBuilder app)
    {
        RatingsEndpoints.Map(app);
        return app;
    }
}

