using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Validate security configuration early in startup
        SecurityExtensions.ValidateSecurityConfiguration(configuration, environment);

        // Registro da configuração de Rate Limit com validação
        services.AddSingleton(provider =>
        {
            var options = new RateLimitOptions();
            configuration.GetSection(RateLimitOptions.SectionName).Bind(options);
            
            // Validações básicas para a configuração avançada
            if (options.Anonymous.RequestsPerMinute <= 0)
                throw new InvalidOperationException("Anonymous RequestsPerMinute must be greater than zero");
            if (options.Authenticated.RequestsPerMinute <= 0)
                throw new InvalidOperationException("Authenticated RequestsPerMinute must be greater than zero");
            if (options.General.WindowInSeconds <= 0)
                throw new InvalidOperationException("WindowInSeconds must be greater than zero");
                
            return options;
        });

        services.AddDocumentation();
        services.AddApiVersioning(); // Adicionar versionamento de API
        services.AddCorsPolicy(configuration, environment);
        services.AddMemoryCache();
        
        // Adicionar serviços de autenticação básica (required for middleware)
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
                options.DefaultScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
            {
                // Configure basic JWT settings - can be enhanced later
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireExpirationTime = false,
                    ClockSkew = TimeSpan.Zero
                };
                options.RequireHttpsMetadata = false;
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Basic token validation logic can be added here
                        return Task.CompletedTask;
                    }
                };
            });
        
        // Adicionar serviços de autorização
        services.AddAuthorizationPolicies();
        
        // Otimizações de performance
        services.AddResponseCompression();
        services.AddStaticFilesWithCaching();
        services.AddApiResponseCaching();
        
        // Serviços específicos por ambiente
        services.AddEnvironmentSpecificServices(configuration, environment);

        return services;
    }

    public static IApplicationBuilder UseApiServices(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        // Middlewares de performance devem estar no início do pipeline
        app.UseResponseCompression();
        app.UseResponseCaching();
        
        // Middleware de arquivos estáticos com cache
        app.UseMiddleware<StaticFilesMiddleware>();
        app.UseStaticFiles();
        
        // Middlewares específicos por ambiente
        app.UseEnvironmentSpecificMiddlewares(environment);
        
        app.UseApiMiddlewares();

        // Documentação apenas em desenvolvimento e testes
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            app.UseDocumentation();
        }

        app.UseCors("DefaultPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}