using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using MeAjudaAi.ApiService.Endpoints;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.ApiService.Services.Authentication;
using MeAjudaAi.Shared.Authorization.Middleware;
using MeAjudaAi.Shared.Logging;
using MeAjudaAi.Shared.Middleware;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.ApiService.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Detecta se estamos em ambiente de teste (integração ou E2E)
        var isTestEnvironment = string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase) ||
                               environment.IsEnvironment("Testing");

        services.AddDocumentation();
        services.AddApiVersioning(); // Adiciona versionamento de API
        services.AddCustomAntiforgery();
        services.AddMemoryCache();

        // Configura ForwardedHeaders para suporte a proxy reverso (load balancers, nginx, etc.)
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                      Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

            // Limpa redes e proxies padrão - será configurado por ambiente
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            // Em produção, configure KnownProxies ou KnownIPNetworks com os IPs do seu proxy reverso
            // Exemplo para Docker/Kubernetes:
            // options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
        });

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
        services.AddAuthorizationPolicies(configuration, environment);

        // Adiciona suporte a ProblemDetails para respostas de erro padronizadas
        services.AddProblemDetails();

        // Otimizações de performance
        services.AddResponseCompression();
        services.AddStaticFilesWithCaching();
        services.AddApiResponseCaching();

        // Health Checks customizados
        services.AddMeAjudaAiHealthChecks();

        // Health Checks UI removido - usar Aspire Dashboard (http://localhost:15888)
        // A Aspire Dashboard fornece visualização avançada de health checks, métricas, traces e logs
        // em uma interface unificada e moderna, tornando o Health Checks UI redundante

        // Serviços específicos por ambiente
        services.AddEnvironmentSpecificServices(configuration, environment);

        return services;
    }

    public static WebApplication UseApiServices(
        this WebApplication app,
        IWebHostEnvironment environment)
    {
        // Exception handling DEVE estar no início do pipeline
        app.UseExceptionHandler();

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            app.UseHsts();
        }

        // ForwardedHeaders deve ser o primeiro para popular corretamente RemoteIpAddress para rate limiting
        // Processa cabeçalhos X-Forwarded-* de proxies reversos (load balancers, nginx, etc.)
        app.UseForwardedHeaders();

        app.UseHttpsRedirection();

        // Content Security Policy - adicionar no início para proteger todas as respostas
        app.UseContentSecurityPolicy();

        // Logging Context Middleware - adiciona correlation ID aos logs e response headers
        app.UseLoggingContext();

        // Verificação de segurança de compressão (previne CRIME/BREACH)
        // DEVE estar ANTES de UseResponseCompression() para poder desabilitar compressão
        // quando necessário. Usa detecção de headers de autenticação pois executa
        // antes de UseAuthentication().
        app.UseMiddleware<CompressionSecurityMiddleware>();

        // Middlewares de performance devem estar no início do pipeline
        app.UseResponseCompression();
        app.UseResponseCaching();

        // Middleware de arquivos estáticos com cache
        app.UseMiddleware<StaticFilesMiddleware>();
        app.UseStaticFiles();

        // Middlewares específicos por ambiente
        app.UseEnvironmentSpecificMiddlewares(environment);

        app.UseApiMiddlewares();

        // Geographic restriction (deve rodar antes de authentication para permitir 451 sem auth)
        app.UseMiddleware<GeographicRestrictionMiddleware>();

        // Documentação apenas em desenvolvimento e testes
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            app.UseDocumentation();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        
        // Middleware para expor o cookie de antiforgery em requisições GET (Hardening)
        app.UseMiddleware<AntiforgeryCookieMiddleware>();

        // Debug Middleware para diagnóstico de autorização (apenas em desenvolvimento)
        if (app.Environment.IsDevelopment())
        {
            app.UseMiddleware<InspectAuthMiddleware>();
        }

        // Log de requisições (após autenticação para capturar userId dos claims)
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UsePermissionOptimization(); // Middleware de otimização após autenticação

        // Mapear endpoints de configuração (deve ser chamado após UseAuthorization)
        app.MapConfigurationEndpoints();

        // Map CSP Report Endpoint (deve ser anônimo)
        app.MapCspReportEndpoints();

        // Health Checks UI removido - usar Aspire Dashboard (http://localhost:15888)
        // Para visualizar health checks, acesse o Aspire Dashboard que oferece:
        // - Visualização em tempo real do status de todos os serviços
        // - Histórico e tendências de saúde dos componentes
        // - Integração com logs, traces e métricas
        // - Interface moderna e responsiva

        return app;
    }
}
