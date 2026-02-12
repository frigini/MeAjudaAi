using System.Reflection;
using MeAjudaAi.Architecture.Tests.Helpers;

#pragma warning disable xUnit1004 // Teste documentacional ignorado intencionalmente

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Testes de fronteiras de módulos garantindo isolamento adequado entre módulos
/// Crítico para integridade da arquitetura de monólito modular
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly IEnumerable<ModuleInfo> AllModules = ModuleDiscoveryHelper.DiscoverModules();

    [Fact]
    public void Modules_ShouldNotReference_OtherModules()
    {
        // Os módulos não devem referenciar diretamente outros módulos
        // A comunicação deve acontecer apenas através de eventos de integração
        var failures = new List<string>();

        foreach (var currentModule in AllModules)
        {
            var otherModuleNames = AllModules
                .Where(m => m.Name != currentModule.Name)
                .Select(m => $"MeAjudaAi.Modules.{m.Name}")
                .ToArray();

            if (otherModuleNames.Length == 0) continue; // Não há outros módulos para testar

            var assembliesInModule = new[]
            {
                currentModule.ApiAssembly,
                currentModule.ApplicationAssembly,
                currentModule.InfrastructureAssembly,
                currentModule.DomainAssembly
            }.Where(a => a != null).ToArray();

            foreach (var assembly in assembliesInModule)
            {
                var result = Types.InAssembly(assembly!)
                    .Should()
                    .NotHaveDependencyOnAny(otherModuleNames)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    var assemblyLayer = GetLayerName(assembly!, currentModule);
                    var violationDetails = result.FailingTypes?.Select(t =>
                        $"{currentModule.Name}.{assemblyLayer}: {t.FullName}") ?? [];
                    failures.AddRange(violationDetails);
                }
            }
        }

        failures.Should().BeEmpty(
            "Módulos não devem referenciar outros módulos diretamente. " +
            "Violações: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Module_Internal_Types_ShouldNotBePublic()
    {
        // Implementações internas do módulo não devem ser expostas publicamente
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            if (module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.InfrastructureAssembly)
                .That()
                .ResideInNamespaceContaining(".Persistence.Repositories")
                .Or()
                .ResideInNamespaceContaining(".Services")
                .Or()
                .ResideInNamespaceContaining(".Events.Handlers")
                .Should()
                .NotBePublic()
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Implementações internas do módulo não devem ser públicas. " +
            "Violações: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Module_Domain_ShouldOnlyDependOn_Shared()
    {
        // O domínio do módulo deve depender apenas de abstrações compartilhadas
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            if (module.DomainAssembly == null) continue;

            var referencedAssemblies = module.DomainAssembly.GetReferencedAssemblies()
                .Where(a => a.Name?.StartsWith("MeAjudaAi") == true)
                .Select(a => a.Name)
                .ToList();

            var invalidReferences = referencedAssemblies
                .Where(name => name != "MeAjudaAi.Shared" &&
                              !name?.StartsWith("System") == true &&
                              !name?.StartsWith("Microsoft") == true)
                .ToList();

            if (invalidReferences.Any())
            {
                failures.Add($"{module.Name}: {string.Join(", ", invalidReferences)}");
            }
        }

        failures.Should().BeEmpty(
            "Domínio deve referenciar apenas o projeto Shared e assemblies do framework. " +
            "Referências inválidas: {0}",
            string.Join("; ", failures));
    }

    [Fact(Skip = "LIMITAÇÃO TÉCNICA: DbContext deve ser público para ferramentas de design-time do EF Core, mas conceitualmente deveria ser internal")]
    public void Module_DbContext_ShouldBeInternal()
    {
        // Conceitualmente, DbContext deveria ser internal para melhor encapsulamento do módulo
        // Porém, o EF Core exige que seja público para suas ferramentas de design-time funcionarem
        // Este teste documenta a arquitetura ideal, mesmo que não possa ser aplicada devido à limitação técnica
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            if (module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("DbContext")
                .Should()
                .NotBePublic()
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.Name}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "DbContext deveria ser internal ao módulo para melhor encapsulamento. " +
            "Tipos DbContext públicos encontrados: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Module_DbContext_ShouldNotBeReferencedOutsideInfrastructure()
    {
        // DbContext não deve ser referenciado fora da camada de Infrastructure
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            if (module.InfrastructureAssembly == null) continue;

            var dbContextTypeNames = Types.InAssembly(module.InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("DbContext")
                .GetTypes()
                .Select(t => t.FullName!)
                .ToArray();

            if (!dbContextTypeNames.Any()) continue;

            // Testar camada Domain
            if (module.DomainAssembly != null)
            {
                var domainResult = Types.InAssembly(module.DomainAssembly)
                    .Should()
                    .NotHaveDependencyOnAll(dbContextTypeNames)
                    .GetResult();

                if (!domainResult.IsSuccessful)
                {
                    failures.AddRange(domainResult.FailingTypes?.Select(t => $"{module.Name}.Domain: {t.Name}") ?? []);
                }
            }

            // Testar camada Application
            if (module.ApplicationAssembly != null)
            {
                var applicationResult = Types.InAssembly(module.ApplicationAssembly)
                    .Should()
                    .NotHaveDependencyOnAll(dbContextTypeNames)
                    .GetResult();

                if (!applicationResult.IsSuccessful)
                {
                    failures.AddRange(applicationResult.FailingTypes?.Select(t => $"{module.Name}.Application: {t.Name}") ?? []);
                }
            }

            // Testar camada API
            if (module.ApiAssembly != null)
            {
                var apiResult = Types.InAssembly(module.ApiAssembly)
                    .Should()
                    .NotHaveDependencyOnAll(dbContextTypeNames)
                    .GetResult();

                if (!apiResult.IsSuccessful)
                {
                    failures.AddRange(apiResult.FailingTypes?.Select(t => $"{module.Name}.API: {t.Name}") ?? []);
                }
            }
        }

        failures.Should().BeEmpty(
            "DbContext não deve ser referenciado fora da camada Infrastructure. " +
            "Violações encontradas: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Module_Extensions_ShouldBePublic()
    {
        // Classes de extensão para registro de DI devem ser públicas
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            if (module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("Extensions")
                .Should()
                .BePublic()
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Classes de extensão devem ser públicas para registro de DI. " +
            "Violações: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Integration_Events_ShouldBeInSharedProject()
    {
        // Eventos de integração devem estar no projeto Shared para comunicação entre módulos
        var failures = new List<string>();

        foreach (var module in AllModules)
        {
            var assembliesInModule = new[]
            {
                module.ApiAssembly,
                module.ApplicationAssembly,
                module.InfrastructureAssembly,
                module.DomainAssembly
            }.Where(a => a != null).ToArray();

            foreach (var assembly in assembliesInModule)
            {
                var integrationEventTypes = Types.InAssembly(assembly!)
                    .That()
                    .HaveNameEndingWith("IntegrationEvent")
                    .GetTypes();

                if (integrationEventTypes.Any())
                {
                    var assemblyLayer = GetLayerName(assembly!, module);
                    failures.AddRange(integrationEventTypes.Select(t =>
                        $"{module.Name}.{assemblyLayer}: {t.FullName}"));
                }
            }
        }

        failures.Should().BeEmpty(
            "Eventos de integração não devem existir em assemblies de módulo, devem estar no Shared. " +
            "Encontrados: {0}",
            string.Join(", ", failures));
    }

    private static string GetLayerName(Assembly assembly, ModuleInfo module)
    {
        if (assembly == module.ApiAssembly) return "API";
        if (assembly == module.ApplicationAssembly) return "Application";
        if (assembly == module.InfrastructureAssembly) return "Infrastructure";
        if (assembly == module.DomainAssembly) return "Domain";
        return "Unknown";
    }
}
