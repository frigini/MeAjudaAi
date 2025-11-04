using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para configurar autenticação em testes
/// </summary>
public static class TestAuthenticationExtensions
{
    /// <summary>
    /// Adiciona autenticação configurável para testes específicos
    /// Permite configurar usuários dinamicamente durante os testes
    /// </summary>
    public static IServiceCollection AddConfigurableTestAuthentication(this IServiceCollection services)
    {
        return services.AddAuthentication(ConfigurableTestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ConfigurableTestAuthenticationHandler>(
                ConfigurableTestAuthenticationHandler.SchemeName, _ => { })
            .Services;
    }

    /// <summary>
    /// Adiciona autenticação baseada em instância para testes - elimina problemas de estado estático
    /// Cada factory de teste obtém sua própria configuração de autenticação isolada
    /// </summary>
    public static IServiceCollection AddInstanceTestAuthentication(this IServiceCollection services)
    {
        // Register the configuration as a singleton per test factory instance
        services.AddSingleton<ITestAuthenticationConfiguration, TestAuthenticationConfiguration>();

        return services.AddAuthentication(InstanceTestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, InstanceTestAuthenticationHandler>(
                InstanceTestAuthenticationHandler.SchemeName, _ => { })
            .Services;
    }

    /// <summary>
    /// Adiciona autenticação para testes Aspire
    /// Autentica baseado na presença do Authorization header
    /// </summary>
    public static IServiceCollection AddAspireTestAuthentication(this IServiceCollection services)
    {
        return services.AddAuthentication(AspireTestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, AspireTestAuthenticationHandler>(
                AspireTestAuthenticationHandler.SchemeName, _ => { })
            .Services;
    }

    /// <summary>
    /// Adiciona autenticação para desenvolvimento que sempre autentica como admin
    /// ⚠️ APENAS PARA DESENVOLVIMENTO - NUNCA EM PRODUÇÃO ⚠️
    /// </summary>
    public static IServiceCollection AddDevelopmentTestAuthentication(this IServiceCollection services)
    {
        return services.AddAuthentication(DevelopmentTestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, DevelopmentTestAuthenticationHandler>(
                DevelopmentTestAuthenticationHandler.SchemeName, _ => { })
            .Services;
    }

    /// <summary>
    /// Remove serviços de autenticação específicos que interferem com testes
    /// Útil para substituir autenticação real por mock em testes
    /// </summary>
    public static IServiceCollection RemoveRealAuthentication(this IServiceCollection services)
    {
        // Remove apenas os handlers específicos que podem interferir, não todos os serviços de auth
        var handlersToRemove = services.Where(s =>
            s.ImplementationType?.Name.Contains("TestAuthenticationHandler") == true ||
            s.ImplementationType?.Name.Contains("FakeIntegrationAuthenticationHandler") == true ||
            s.ServiceType?.Name.Contains("JwtBearer") == true ||
            s.ServiceType?.Name.Contains("Bearer") == true && !s.ServiceType?.Name.Contains("Authorization") == true
        ).ToList();

        foreach (var service in handlersToRemove)
        {
            services.Remove(service);
        }

        // Authentication handlers removidos para substituição por handlers de teste

        return services;
    }
}
