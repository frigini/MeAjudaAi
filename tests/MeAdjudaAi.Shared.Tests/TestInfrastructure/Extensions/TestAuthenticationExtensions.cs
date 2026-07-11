using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para configurar autenticação em testes
/// </summary>
public static class TestAuthenticationExtensions
{
    /// <summary>
    /// Adiciona autenticação baseada em instância para testes - elimina problemas de estado estático
    /// Cada factory de teste obtém sua própria configuração de autenticação isolada
    /// </summary>
    public static IServiceCollection AddInstanceTestAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<ITestAuthenticationConfiguration, TestAuthenticationConfiguration>();

        return services.AddAuthentication(InstanceTestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, InstanceTestAuthenticationHandler>(
                InstanceTestAuthenticationHandler.SchemeName, _ => { })
            .Services;
    }

    /// <summary>
    /// Remove serviços de autenticação específicos que interferem com testes
    /// Útil para substituir autenticação real por mock em testes
    /// </summary>
    public static IServiceCollection RemoveRealAuthentication(this IServiceCollection services)
    {
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

        return services;
    }
}
