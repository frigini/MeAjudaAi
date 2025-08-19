using MeAjudaAi.Modules.Users.API.Endpoints;
using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.API;

public static class Extensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static WebApplication UseUsersModule(this WebApplication app)
    {
        app.MapUsersEndpoints();

        return app;
    }
}
