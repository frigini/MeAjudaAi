using MeAjudaAi.Modules.Documents.API.Endpoints;
using MeAjudaAi.Modules.Documents.Application;
using MeAjudaAi.Modules.Documents.Infrastructure;
using MeAjudaAi.Contracts.Modules.Documents;

namespace MeAjudaAi.Modules.Documents.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Documents.
    /// </summary>
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration, environment);

        // Register module public API for cross-module communication
        services.AddScoped<IDocumentsModuleApi, Application.ModuleApi.DocumentsModuleApi>();

        return services;
    }

    /// <summary>
    /// Configura os endpoints do módulo Documents.
    /// </summary>
    public static WebApplication UseDocumentsModule(this WebApplication app)
    {
        app.MapDocumentsEndpoints();

        return app;
    }
}
