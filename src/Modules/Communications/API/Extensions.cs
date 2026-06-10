using MeAjudaAi.Modules.Communications.API.Endpoints;
using MeAjudaAi.Modules.Communications.Application;
using MeAjudaAi.Modules.Communications.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API;

[ExcludeFromCodeCoverage]
public static class CommunicationsModuleExtensions
{
    public static IServiceCollection AddCommunicationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static WebApplication UseCommunicationsModule(this WebApplication app)
    {
        app.MapCommunicationsEndpoints();
        return app;
    }

    public static IEndpointRouteBuilder MapCommunicationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        CommunicationsEndpoints.Map(endpoints);
        return endpoints;
    }
}
