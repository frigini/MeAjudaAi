using MeAjudaAi.Modules.Communications.Application;
using MeAjudaAi.Modules.Communications.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Communications.API;

public static class Extensions
{
    public static IServiceCollection AddCommunicationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommunicationsApplication();
        services.AddCommunicationsInfrastructure(configuration);

        return services;
    }

    public static IApplicationBuilder UseCommunicationsModule(this IApplicationBuilder app)
    {
        // Registro de middlewares específicos do módulo, se houver
        return app;
    }

    public static IEndpointRouteBuilder MapCommunicationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/communications")
            .WithTags("Communications")
            .RequireAuthorization();

        // Endpoints will be mapped here
        // group.MapGet("/templates", ...);
        // group.MapGet("/logs", ...);

        return endpoints;
    }
}
