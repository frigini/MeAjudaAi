using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para registrar serviços de teste usando discovery de tipos
/// Facilita registro de mocks, stubs e test doubles
/// </summary>
public static class TestServiceRegistrationExtensions
{
    /// <summary>
    /// Adiciona todos os mocks de um assembly seguindo convenções de nomenclatura
    /// Procura por classes que terminam com "Mock" ou implementam interfaces que começam com "IMock"
    /// </summary>
    public static IServiceCollection AddTestMocks(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Mock") || 
                              type.GetInterfaces().Any(i => i.Name.StartsWith("IMock"))))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// Adiciona todos os test doubles (stubs, fakes, etc.) de um assembly
    /// Procura por classes que terminam com "Stub", "Fake", "TestDouble"
    /// </summary>
    public static IServiceCollection AddTestDoubles(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Stub") || 
                              type.Name.EndsWith("Fake") || 
                              type.Name.EndsWith("TestDouble")))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// Adiciona builders/factories para testes seguindo convenções
    /// Procura por classes que terminam com "Builder", "Factory" em namespaces de teste
    /// </summary>
    public static IServiceCollection AddTestBuilders(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => (type.Name.EndsWith("Builder") || type.Name.EndsWith("Factory")) &&
                              type.Namespace != null && type.Namespace.Contains("Test")))
            .AsSelf()
            .WithTransientLifetime());
    }

    /// <summary>
    /// Adiciona helpers de teste seguindo convenções
    /// Procura por classes que terminam com "Helper", "TestHelper", "TestUtility"
    /// </summary>
    public static IServiceCollection AddTestHelpers(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Helper") || 
                              type.Name.EndsWith("TestHelper") || 
                              type.Name.EndsWith("TestUtility")))
            .AsSelf()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// Adiciona fixtures de teste seguindo convenções
    /// Procura por classes que terminam com "Fixture"
    /// </summary>
    public static IServiceCollection AddTestFixtures(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Fixture")))
            .AsSelf()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// Adiciona todas as implementações de teste de uma interface específica
    /// Útil para registrar todos os mocks/fakes que implementam uma interface comum
    /// </summary>
    public static IServiceCollection AddTestImplementationsOf<TInterface>(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo<TInterface>()
                .Where(type => type.Namespace != null && type.Namespace.Contains("Test")))
            .As<TInterface>()
            .WithSingletonLifetime());
    }

    /// <summary>
    /// Método conveniente para registrar todos os tipos de teste de uma vez
    /// </summary>
    public static IServiceCollection AddAllTestServices(this IServiceCollection services, Assembly assembly)
    {
        return services
            .AddTestMocks(assembly)
            .AddTestDoubles(assembly)
            .AddTestBuilders(assembly)
            .AddTestHelpers(assembly)
            .AddTestFixtures(assembly);
    }

    /// <summary>
    /// Adiciona todos os handlers de teste (para eventos, comandos, queries de teste)
    /// Procura por classes que terminam com "TestHandler" ou contêm "Test" no namespace
    /// </summary>
    public static IServiceCollection AddTestHandlers(this IServiceCollection services, Assembly assembly)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("TestHandler") ||
                              type.Name.EndsWith("MockHandler") ||
                              (type.Namespace != null && type.Namespace.Contains("Test") && 
                               type.Name.EndsWith("Handler"))))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    /// <summary>
    /// Adiciona services específicos para um módulo de teste
    /// Procura por classes em namespaces que contêm o nome do módulo e "Test"
    /// </summary>
    public static IServiceCollection AddModuleTestServices(this IServiceCollection services, 
        Assembly assembly, string moduleName)
    {
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .Where(type => type.Namespace != null && 
                              type.Namespace.Contains(moduleName, StringComparison.OrdinalIgnoreCase) &&
                              type.Namespace.Contains("Test")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}