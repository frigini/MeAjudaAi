using MeAjudaAi.Modules.Communications.API.Endpoints;
using MeAjudaAi.Modules.Communications.Application;
using MeAjudaAi.Modules.Communications.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddCommunicationsModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseCommunicationsModule(this IEndpointRouteBuilder app)
    {
        CommunicationsEndpoints.Map(app);
        return app;
    }
}