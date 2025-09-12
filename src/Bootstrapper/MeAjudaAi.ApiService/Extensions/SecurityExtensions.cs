using MeAjudaAi.ApiService.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace MeAjudaAi.ApiService.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
                var realm = configuration["Keycloak:Realm"];
                
                options.Authority = $"{keycloakBaseUrl}/realms/{realm}";
                options.Audience = configuration["Keycloak:ClientId"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false, // Keycloak doesn't use audience by default
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "roles" // Keycloak uses 'roles' claim
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("admin", "super-admin"))
            .AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("super-admin"))
            .AddPolicy("UserManagement", policy =>
                policy.RequireRole("admin", "super-admin"))
            .AddPolicy("ServiceProviderAccess", policy =>
                policy.RequireRole("service-provider", "admin", "super-admin"))
            .AddPolicy("CustomerAccess", policy =>
                policy.RequireRole("customer", "admin", "super-admin"))
            .AddPolicy("SelfOrAdmin", policy =>
                policy.AddRequirements(new SelfOrAdminRequirement()));

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, SelfOrAdminHandler>();

        return services;
    }
}