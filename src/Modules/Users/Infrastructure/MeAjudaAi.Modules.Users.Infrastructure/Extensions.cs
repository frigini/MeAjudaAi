using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Keycloak
        services.Configure<KeycloakOptions>(
            configuration.GetSection(KeycloakOptions.SectionName));

        services.AddHttpClient<IKeycloakService, KeycloakServicet>();

        // Database - Direct DbContext usage (no Repository pattern)
        services.AddPostgresContext<UsersDbContext>();

        // Application Services - Implemented in Infrastructure to avoid circular dependencies
        services.AddScoped<IUserService, UserService>();

        // Event Handlers - The shared Events extension will automatically discover and register
        // all IEventHandler<T> implementations from this assembly via Scrutor
        // No need to manually register each handler

        return services;
    }
}