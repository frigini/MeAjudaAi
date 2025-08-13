using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Messaging;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
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

        // Database
        services.AddPostgresContext<UsersDbContext>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Messaging
        services.AddScoped<IUserEventPublisher, UserEventPublisher>();

        return services;
    }
}
