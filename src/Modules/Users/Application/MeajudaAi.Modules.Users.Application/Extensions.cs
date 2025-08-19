using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IKeycloakService, KeycloakService>();
        //services.AddScoped<IUserManagementService, UserManagementService>();
        //services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
        //services.AddScoped<ITokenValidationService, TokenValidationService>();

        return services;
    }
}
