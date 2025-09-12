using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddKeycloak(configuration);
        services.AddDomainServices();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("meajudaai-db");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure")));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddKeycloak(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.AddHttpClient<IKeycloakService, KeycloakService>();

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserDomainService, KeycloakUserDomainService>();
        services.AddScoped<IAuthenticationDomainService, KeycloakAuthenticationDomainService>();

        return services;
    }
}