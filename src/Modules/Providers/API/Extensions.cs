using MeAjudaAi.Modules.Providers.API.Endpoints;
using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Providers.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços para encadeamento</returns>
    public static IServiceCollection AddProvidersModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Providers.
    /// </summary>
    /// <param name="app">Aplicação web</param>
    /// <returns>Aplicação web para encadeamento</returns>
    public static WebApplication UseProvidersModule(this WebApplication app)
    {
        app.MapProvidersEndpoints();

        return app;
    }
}
