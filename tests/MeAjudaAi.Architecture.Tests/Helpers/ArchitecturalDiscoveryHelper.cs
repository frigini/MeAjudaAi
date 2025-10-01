using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests.Helpers;

/// <summary>
/// Helper que usa descoberta automática de tipos com convenções arquiteturais
/// Simplifica a descoberta e validação de padrões de design usando diferentes estratégias
/// </summary>
public static class ArchitecturalDiscoveryHelper
{
    /// <summary>
    /// Descobre todos os Command Handlers usando discovery automático para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverCommandHandlers()
    {
        var services = new ServiceCollection();
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();

        foreach (var assembly in allApplicationAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Handler") &&
                                  type.GetInterfaces().Any(i =>
                                      i.IsGenericType &&
                                      i.GetGenericTypeDefinition().Name.Contains("ICommandHandler"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Query Handlers usando discovery automático para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverQueryHandlers()
    {
        var services = new ServiceCollection();
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();

        foreach (var assembly in allApplicationAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Handler") &&
                                  type.GetInterfaces().Any(i =>
                                      i.IsGenericType &&
                                      i.GetGenericTypeDefinition().Name.Contains("IQueryHandler"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Event Handlers usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverEventHandlers()
    {
        var services = new ServiceCollection();
        var allInfraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();

        foreach (var assembly in allInfraAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Handler") &&
                                  type.GetInterfaces().Any(i =>
                                      i.IsGenericType &&
                                      i.GetGenericTypeDefinition().Name.Contains("IEventHandler"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Domain Events usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverDomainEvents()
    {
        var services = new ServiceCollection();
        var allDomainAssemblies = ModuleDiscoveryHelper.GetAllDomainAssemblies();

        foreach (var assembly in allDomainAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.GetInterfaces().Any(i =>
                           i.Name.Contains("IDomainEvent"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Commands usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverCommands()
    {
        var services = new ServiceCollection();
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();

        foreach (var assembly in allApplicationAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.GetInterfaces().Any(i =>
                           i.Name.Contains("ICommand"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Queries usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverQueries()
    {
        var services = new ServiceCollection();
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();

        foreach (var assembly in allApplicationAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.GetInterfaces().Any(i =>
                           i.Name.Contains("IQuery"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Entities usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverEntities()
    {
        var services = new ServiceCollection();
        var allDomainAssemblies = ModuleDiscoveryHelper.GetAllDomainAssemblies();

        foreach (var assembly in allDomainAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Entity") ||
                                  type.GetInterfaces().Any(i => i.Name.Contains("IEntity"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Value Objects usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverValueObjects()
    {
        var services = new ServiceCollection();
        var allDomainAssemblies = ModuleDiscoveryHelper.GetAllDomainAssemblies();

        foreach (var assembly in allDomainAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("ValueObject") ||
                                  type.GetInterfaces().Any(i => i.Name.Contains("IValueObject"))))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre todos os Repositories usando Scrutor para validar convenções
    /// </summary>
    public static IEnumerable<Type> DiscoverRepositories()
    {
        var services = new ServiceCollection();
        var allInfraAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();

        foreach (var assembly in allInfraAssemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Repository") &&
                                  !type.IsInterface))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Descobre tipos por convenção personalizada usando discovery automático
    /// </summary>
    public static IEnumerable<Type> DiscoverTypesByConvention(
        IEnumerable<Assembly> assemblies,
        Func<Type, bool> typeFilter)
    {
        var services = new ServiceCollection();

        foreach (var assembly in assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(typeFilter))
                .AsSelf());
        }

        return services
            .Where(sd => sd.ServiceType == sd.ImplementationType)
            .Select(sd => sd.ImplementationType!)
            .Distinct();
    }

    /// <summary>
    /// Valida se todos os tipos descobertos seguem uma convenção de nomenclatura
    /// </summary>
    public static (bool IsValid, IEnumerable<string> Violations) ValidateNamingConvention(
        IEnumerable<Type> types,
        string expectedSuffix,
        string violationMessage)
    {
        var violations = types
            .Where(type => !type.Name.EndsWith(expectedSuffix))
            .Select(type => $"{type.FullName} should end with '{expectedSuffix}'")
            .ToList();

        return (violations.Count == 0, violations);
    }

    /// <summary>
    /// Valida se todos os tipos descobertos implementam uma interface esperada
    /// </summary>
    public static (bool IsValid, IEnumerable<string> Violations) ValidateInterfaceImplementation(
        IEnumerable<Type> types,
        Type expectedInterface,
        string violationMessage)
    {
        var violations = types
            .Where(type => !type.GetInterfaces().Contains(expectedInterface))
            .Select(type => $"{type.FullName} should implement {expectedInterface.Name}")
            .ToList();

        return (violations.Count == 0, violations);
    }
}