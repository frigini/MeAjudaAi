using System.Security.Claims;
using System.Text.Encodings.Web;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Authorization.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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

        // Registro da configuração de Rate Limit com validação usando Options pattern
        // Suporte tanto para nova seção "AdvancedRateLimit" quanto para legado "RateLimit"
        var optionsBuilder = services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.SectionName) // "AdvancedRateLimit"
            .BindConfiguration("RateLimit") // fallback para configuração legada
            .ValidateDataAnnotations(); // Valida atributos [Required] etc.
        
        // Apenas valida na inicialização se NÃO estiver em ambiente de teste
        var isTestEnvironment = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true" 
                               || environment.IsEnvironment("Testing");
        
        if (!isTestEnvironment)
        {
            optionsBuilder.ValidateOnStart() // Valida na inicialização da aplicação
                .Validate(options =>
                {
                    // Validações customizadas para a configuração avançada
                    if (options.Anonymous.RequestsPerMinute <= 0 || options.Anonymous.RequestsPerHour <= 0 || options.Anonymous.RequestsPerDay <= 0)
                        return false;
                    if (options.Authenticated.RequestsPerMinute <= 0 || options.Authenticated.RequestsPerHour <= 0 || options.Authenticated.RequestsPerDay <= 0)
                        return false;
                    if (options.General.WindowInSeconds <= 0)
                        return false;
                    if (options.General.EnableIpWhitelist && (options.General.WhitelistedIps == null || options.General.WhitelistedIps.Count == 0))
                        return false;
                    return true;
                }, "Rate limit configuration is invalid. All limits must be greater than zero.");
        }

        services.AddDocumentation();
        services.AddApiVersioning(); // Adiciona versionamento de API
        services.AddCorsPolicy(configuration, environment);
        services.AddMemoryCache();

        // Adiciona autenticação segura baseada no ambiente
        // Configuração de autenticação baseada no ambiente
        var it = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.Equals(it, "true", StringComparison.OrdinalIgnoreCase) && !environment.IsEnvironment("Testing"))
        {
            // Usa a extensão segura do Keycloak com validação completa de tokens
            services.AddEnvironmentAuthentication(configuration, environment);
        }
        else
        {
            // Para testing environment, adiciona authentication handler customizado
            services.AddAuthentication("Test")
                .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            services.AddSingleton<IClaimsTransformation, NoOpClaimsTransformation>();
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
        app.UsePermissionOptimization(); // Middleware de otimização após autenticação
        app.UseAuthorization();

        return app;
    }
}

/// <summary>
/// No-op implementation of IClaimsTransformation for cases where minimal transformation is needed
/// </summary>
public class NoOpClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        return Task.FromResult(principal);
    }
}
