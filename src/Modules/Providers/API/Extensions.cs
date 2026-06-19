using MeAjudaAi.Modules.Providers.API.Endpoints;
using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseProvidersModule(this IEndpointRouteBuilder app)
    {
        ProvidersModuleEndpoints.Map(app);
        return app;
    }
}

