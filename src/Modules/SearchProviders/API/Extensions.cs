using MeAjudaAi.Modules.SearchProviders.API.Endpoints;
using MeAjudaAi.Modules.SearchProviders.Application;
using MeAjudaAi.Modules.SearchProviders.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddSearchProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseSearchProvidersModule(this IEndpointRouteBuilder app)
    {
        SearchProvidersEndpoint.Map(app);
        return app;
    }
}