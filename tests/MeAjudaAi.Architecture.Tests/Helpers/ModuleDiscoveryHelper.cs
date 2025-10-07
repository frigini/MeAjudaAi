using System.Reflection;

namespace MeAjudaAi.Architecture.Tests.Helpers;

/// <summary>
/// Helper para descoberta automática de módulos e seus assemblies
/// Permite que os testes de arquitetura sejam genéricos e suportem novos módulos automaticamente
/// </summary>
public static class ModuleDiscoveryHelper
{
    /// <summary>
    /// Descobre todos os módulos disponíveis na solução
    /// </summary>
    public static IEnumerable<ModuleInfo> DiscoverModules()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic &&
                       a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .ToList();

        var moduleGroups = loadedAssemblies
            .GroupBy(ExtractModuleName)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToList();

        return [.. moduleGroups.Select(group => new ModuleInfo
        {
            Name = group.Key!,
            DomainAssembly = group.FirstOrDefault(a => a.FullName?.Contains(".Domain") == true),
            ApplicationAssembly = group.FirstOrDefault(a => a.FullName?.Contains(".Application") == true),
            InfrastructureAssembly = group.FirstOrDefault(a => a.FullName?.Contains(".Infrastructure") == true),
            ApiAssembly = group.FirstOrDefault(a => a.FullName?.Contains(".API") == true)
        })
        .Where(m => m.DomainAssembly != null)];
    }

    /// <summary>
    /// Obtém todos os assemblies de domínio de todos os módulos
    /// </summary>
    public static IEnumerable<Assembly> GetAllDomainAssemblies()
    {
        return [.. DiscoverModules()
            .Where(m => m.DomainAssembly != null)
            .Select(m => m.DomainAssembly!)];
    }

    /// <summary>
    /// Obtém todos os assemblies de aplicação de todos os módulos
    /// </summary>
    public static IEnumerable<Assembly> GetAllApplicationAssemblies()
    {
        return [.. DiscoverModules()
            .Where(m => m.ApplicationAssembly != null)
            .Select(m => m.ApplicationAssembly!)];
    }

    /// <summary>
    /// Obtém todos os assemblies de infraestrutura de todos os módulos
    /// </summary>
    public static IEnumerable<Assembly> GetAllInfrastructureAssemblies()
    {
        return [.. DiscoverModules()
            .Where(m => m.InfrastructureAssembly != null)
            .Select(m => m.InfrastructureAssembly!)];
    }

    /// <summary>
    /// Obtém todos os assemblies de API de todos os módulos
    /// </summary>
    public static IEnumerable<Assembly> GetAllApiAssemblies()
    {
        return [.. DiscoverModules()
            .Where(m => m.ApiAssembly != null)
            .Select(m => m.ApiAssembly!)];
    }

    /// <summary>
    /// Extrai o nome do módulo do nome completo do assembly
    /// Exemplo: "MeAjudaAi.Modules.Users.Domain" -> "Users"
    /// </summary>
    private static string? ExtractModuleName(Assembly assembly)
    {
        var assemblyName = assembly.FullName?.Split(',')[0];
        if (string.IsNullOrEmpty(assemblyName))
            return null;

        var parts = assemblyName.Split('.');
        var moduleIndex = Array.IndexOf(parts, "Modules");

        if (moduleIndex >= 0 && moduleIndex + 1 < parts.Length)
        {
            return parts[moduleIndex + 1];
        }

        return null;
    }

    /// <summary>
    /// Obtém nomes de assemblies para verificação de dependências
    /// </summary>
    public static IEnumerable<string> GetAssemblyNames(IEnumerable<Assembly> assemblies)
    {
        return [.. assemblies
            .Where(a => a != null)
            .Select(a => a.GetName().Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()];
    }
}

/// <summary>
/// Informações sobre um módulo descoberto
/// </summary>
public class ModuleInfo
{
    public required string Name { get; init; }
    public Assembly? DomainAssembly { get; init; }
    public Assembly? ApplicationAssembly { get; init; }
    public Assembly? InfrastructureAssembly { get; init; }
    public Assembly? ApiAssembly { get; init; }

    public override string ToString() => Name;
}
