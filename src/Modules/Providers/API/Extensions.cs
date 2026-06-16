using MeAjudaAi.Modules.Providers.API.Endpoints;
using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Providers.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Providers.
    /// </summary>
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Providers.
    /// </summary>
    public static WebApplication UseProvidersModule(this WebApplication app)
    {
        app.MapProvidersEndpoints();

        return app;
    }
}

