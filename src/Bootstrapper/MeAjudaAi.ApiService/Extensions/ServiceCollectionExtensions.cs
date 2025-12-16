using System.Security.Claims;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Services.Authentication;
using MeAjudaAi.Shared.Authorization.Middleware;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        // Detecta se estamos em ambiente de teste (integração ou E2E)
        var isTestEnvironment = string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase) ||
                               environment.IsEnvironment("Testing");

        // Registro da configuração de Rate Limit com validação usando Options pattern
        // Suporte tanto para nova seção "AdvancedRateLimit" quanto para legado "RateLimit"
        var optionsBuilder = services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.SectionName) // "AdvancedRateLimit"
            .BindConfiguration("RateLimit") // fallback para configuração legada
            .ValidateDataAnnotations(); // Valida atributos [Required] etc.

        // Apenas valida na inicialização se NÃO estiver em ambiente de teste

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

        // Configurar Geographic Restriction
        services.Configure<GeographicRestrictionOptions>(
            configuration.GetSection("GeographicRestriction"));

        // Configuração de autenticação baseada no ambiente
        if (!isTestEnvironment)
        {
            // Usa a extensão segura do Keycloak com validação completa de tokens
            services.AddEnvironmentAuthentication(configuration, environment);
            services.AddSingleton<IClaimsTransformation, NoOpClaimsTransformation>();
        }
        else
        {
            // Para testing environment, registra um esquema de autenticação mínimo
            // O WebApplicationFactory nos testes substituirá isso com o esquema de teste real
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", _ => { }); // Esquema vazio, será substituído pelo WebApplicationFactory
        }

        // Adiciona serviços de autorização
        services.AddAuthorizationPolicies();

        // Adiciona suporte a ProblemDetails para respostas de erro padronizadas
        services.AddProblemDetails();

        // Otimizações de performance
        services.AddResponseCompression();
        services.AddStaticFilesWithCaching();
        services.AddApiResponseCaching();

        // Health Checks customizados
        services.AddMeAjudaAiHealthChecks(configuration);
        
        // Health Checks UI removido - usar Aspire Dashboard (http://localhost:15888)
        // A Aspire Dashboard fornece visualização avançada de health checks, métricas, traces e logs
        // em uma interface unificada e moderna, tornando o Health Checks UI redundante

        // Serviços específicos por ambiente
        services.AddEnvironmentSpecificServices(configuration, environment);

        return services;
    }

    public static IApplicationBuilder UseApiServices(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        // Exception handling DEVE estar no início do pipeline
        app.UseExceptionHandler();

        // Middlewares de performance devem estar no início do pipeline
        app.UseResponseCompression();
        app.UseResponseCaching();

        // Geographic Restriction ANTES de qualquer roteamento
        app.UseMiddleware<GeographicRestrictionMiddleware>();

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

        // Health Checks UI removido - usar Aspire Dashboard (http://localhost:15888)
        // Para visualizar health checks, acesse o Aspire Dashboard que oferece:
        // - Visualização em tempo real do status de todos os serviços
        // - Histórico e tendências de saúde dos componentes
        // - Integração com logs, traces e métricas
        // - Interface moderna e responsiva

        return app;
    }
}
