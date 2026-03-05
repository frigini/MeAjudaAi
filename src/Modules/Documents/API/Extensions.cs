using MeAjudaAi.Modules.Documents.API.Endpoints;
using MeAjudaAi.Modules.Documents.Application;
using MeAjudaAi.Modules.Documents.Infrastructure;
using MeAjudaAi.Contracts.Modules.Documents;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.API;

public static class Extensions
{
    /// <summary>
    /// Adiciona os serviços do módulo Documents.
    /// </summary>
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

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
