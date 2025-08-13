using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Modules.Users.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, KeycloakService>();
        //services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
        //services.AddScoped<ITokenValidationService, TokenValidationService>();

        return services;
    }
}
