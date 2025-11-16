using MeAjudaAi.Modules.Search.API.Endpoints;
using MeAjudaAi.Modules.Search.Application;
using MeAjudaAi.Modules.Search.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Search.API;

/// <summary>
/// Extensões em nível de módulo para registrar o módulo completo de Search & Discovery.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registra todos os serviços do módulo Search (Domain, Application, Infrastructure, API).
    /// </summary>
    /// <param name="services">A coleção de serviços.</param>
    /// <param name="configuration">A configuração para ler strings de conexão e configurações.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddSearchModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Domain layer não tem dependências externas para registrar

        // Application layer
        services.AddSearchApplication();

        // Infrastructure layer (requer configuration para conexão do BD)
        services.AddSearchInfrastructure(configuration);

        // API layer - sem serviços adicionais para registrar

        return services;
    }

    /// <summary>
    /// Mapeia todos os endpoints do módulo Search.
    /// </summary>
    /// <param name="app">O construtor de rotas de endpoint.</param>
    /// <returns>O construtor de rotas de endpoint para encadeamento.</returns>
    public static IEndpointRouteBuilder UseSearchModule(this IEndpointRouteBuilder app)
    {
        SearchProvidersEndpoint.Map(app);
        return app;
    }
}
