using MeAjudaAi.Modules.Users.API.Endpoints;
using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.API;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder UseUsersModule(this IEndpointRouteBuilder app)
    {
        UsersEndpoints.Map(app);
        return app;
    }
}

