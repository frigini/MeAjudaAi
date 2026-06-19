using MeAjudaAi.Modules.Documents.API.Endpoints;
using MeAjudaAi.Modules.Documents.Application;
using MeAjudaAi.Modules.Documents.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseDocumentsModule(this IEndpointRouteBuilder app)
    {
        DocumentsEndpoints.Map(app);
        return app;
    }
}