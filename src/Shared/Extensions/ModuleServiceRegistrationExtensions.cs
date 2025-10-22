using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para registro automático de serviços de módulos por convenção
/// Facilita o registro consistente de services, repositories, validators, etc.
/// </summary>
public static class ModuleServiceRegistrationExtensions
{
    /// <summary>
    /// Registra todos os services de um módulo seguindo convenções de nomenclatura
    /// </summary>
    public static IServiceCollection AddModuleServices(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Service") &&
                !type.IsInterface &&
                !type.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registra todos os repositories de um módulo seguindo convenções de nomenclatura
    /// </summary>
    public static IServiceCollection AddModuleRepositories(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Repository") &&
                !type.IsInterface &&
                !type.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registra validators do FluentValidation automaticamente
    /// </summary>
    public static IServiceCollection AddModuleValidators(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("Validator") &&
                !type.IsInterface &&
                !type.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registra todos os serviços de cache de um módulo
    /// </summary>
    public static IServiceCollection AddModuleCacheServices(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("CacheService") &&
                !type.IsInterface &&
                !type.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registra todos os Domain Services de um módulo
    /// </summary>
    public static IServiceCollection AddModuleDomainServices(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(type =>
                type.Name.EndsWith("DomainService") &&
                !type.IsInterface &&
                !type.IsAbstract))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
