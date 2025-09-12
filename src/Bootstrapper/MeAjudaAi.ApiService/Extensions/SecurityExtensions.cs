using MeAjudaAi.ApiService.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                options.Authority = configuration["Keycloak:Authority"];
                options.Audience = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = false; // Only for dev
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        _ = services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
            options.AddPolicy("UserManagement", policy =>
                policy.RequireRole("Admin", "SuperAdmin", "UserManager"));
            options.AddPolicy("SelfOrAdmin", policy =>
                policy.AddRequirements(new SelfOrAdminRequirement()));
        });

        return services;
    }
}