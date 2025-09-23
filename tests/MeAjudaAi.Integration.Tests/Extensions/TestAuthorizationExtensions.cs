using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace MeAjudaAi.Integration.Tests.Extensions;

/// <summary>
/// Extensões para configurar authorization handlers automaticamente em testes usando Scrutor
/// </summary>
public static class TestAuthorizationExtensions
{
    /// <summary>
    /// Adiciona automaticamente todos os authorization handlers de teste do assembly atual
    /// </summary>
    public static IServiceCollection AddTestAuthorizationHandlers(this IServiceCollection services)
    {
        return services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes
                .AssignableTo<IAuthorizationHandler>()
                .Where(type => type.Name.EndsWith("Handler") &&
                              (type.Namespace?.Contains("Test") == true || 
                               type.Namespace?.Contains("Integration") == true)))
            .As<IAuthorizationHandler>()
            .WithScopedLifetime());
    }

    /// <summary>
    /// Adiciona automaticamente todos os mocks de serviços do assembly atual
    /// </summary>
    public static IServiceCollection AddTestMocks(this IServiceCollection services)
    {
        return services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes
                .Where(type => type.Name.StartsWith("Mock") && type.IsClass))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}