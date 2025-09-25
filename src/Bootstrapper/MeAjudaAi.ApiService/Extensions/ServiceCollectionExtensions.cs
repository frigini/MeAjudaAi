using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Middlewares;

namespace MeAjudaAi.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Valida a configuração de segurança logo no início do startup
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
        services.AddApiVersioning(); // Adiciona versionamento de API
        services.AddCorsPolicy(configuration, environment);
        services.AddMemoryCache();
        
        // Adiciona serviços de autenticação básica (necessário para o middleware)
        // Para testes de integração (INTEGRATION_TESTS=true), não configuramos JWT Bearer
        // pois será substituído pelo FakeIntegrationAuthenticationHandler
        if (Environment.GetEnvironmentVariable("INTEGRATION_TESTS") != "true")
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Bearer";
                    options.DefaultChallengeScheme = "Bearer";
                    options.DefaultScheme = "Bearer";
                })
                .AddJwtBearer("Bearer", options =>
                {
                    // Configuração básica do JWT - pode ser aprimorada depois
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
                            // Lógica básica de validação do token pode ser adicionada aqui
                            return Task.CompletedTask;
                        }
                    };
                });
        }
        else
        {
            // Para testes de integração, configuramos apenas a base da autenticação
            // O FakeIntegrationAuthenticationHandler será adicionado depois em AddEnvironmentSpecificServices
            services.AddAuthentication();
        }
        
        // Adiciona serviços de autorização
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