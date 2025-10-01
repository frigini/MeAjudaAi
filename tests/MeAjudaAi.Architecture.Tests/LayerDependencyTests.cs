using MeAjudaAi.Architecture.Tests.Helpers;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Testes de dependência de camadas garantindo os princípios da Clean Architecture
/// Baseado nas recomendações de Milan Jovanovic para monólitos modulares
/// </summary>
public class LayerDependencyTests
{
    private static readonly IEnumerable<ModuleInfo> AllModules = ModuleDiscoveryHelper.DiscoverModules();
    private static readonly IEnumerable<Assembly> AllDomainAssemblies = ModuleDiscoveryHelper.GetAllDomainAssemblies();
    private static readonly IEnumerable<Assembly> AllApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();
    private static readonly IEnumerable<Assembly> AllInfrastructureAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
    private static readonly IEnumerable<Assembly> AllApiAssemblies = ModuleDiscoveryHelper.GetAllApiAssemblies();

    [Fact]
    public void Domain_Entities_ShouldBeSealed()
    {
        // Entidades devem ser sealed para evitar problemas de herança
        var failures = new List<string>();

        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Entities")
                .And()
                .AreClasses()
                .Should()
                .BeSealed()
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Domain entities should be sealed. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_Events_ShouldEndWithEvent()
    {
        // Eventos de domínio devem ter sufixo "Event" para clareza
        var failures = new List<string>();

        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Events")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Event")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Domain events should end with 'Event'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_ValueObjects_ShouldBeSealed()
    {
        // Value Objects devem ser sealed para imutabilidade
        var failures = new List<string>();

        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespaceEndingWith(".ValueObjects")
                .And()
                .AreClasses()
                .Should()
                .BeSealed()
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Value objects should be sealed. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_CommandHandlers_ShouldHaveCorrectNaming()
    {
        // Command handlers devem seguir convenção de nomenclatura
        var failures = new List<string>();

        foreach (var applicationAssembly in AllApplicationAssemblies)
        {
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Commands.Handlers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Handler")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.ApplicationAssembly == applicationAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Command handlers should end with 'Handler'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_QueryHandlers_ShouldHaveCorrectNaming()
    {
        // Query handlers devem seguir convenção de nomenclatura
        var failures = new List<string>();

        foreach (var applicationAssembly in AllApplicationAssemblies)
        {
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Queries.Handlers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Handler")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.ApplicationAssembly == applicationAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Query handlers should end with 'Handler'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Infrastructure_Repositories_ShouldHaveCorrectNaming()
    {
        // Repositórios devem seguir convenção de nomenclatura
        var failures = new List<string>();

        foreach (var infrastructureAssembly in AllInfrastructureAssemblies)
        {
            var result = Types.InAssembly(infrastructureAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Repositories")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.InfrastructureAssembly == infrastructureAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Infrastructure repositories should end with 'Repository'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Infrastructure_Configurations_ShouldHaveCorrectNaming()
    {
        // Configurações de EF devem seguir convenção de nomenclatura
        var failures = new List<string>();

        foreach (var infrastructureAssembly in AllInfrastructureAssemblies)
        {
            var result = Types.InAssembly(infrastructureAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Configurations")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Configuration")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.InfrastructureAssembly == infrastructureAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "Infrastructure configurations should end with 'Configuration'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void API_Controllers_ShouldHaveCorrectNaming()
    {
        // Controllers devem seguir convenção de nomenclatura
        var failures = new List<string>();

        foreach (var apiAssembly in AllApiAssemblies)
        {
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Controllers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Controller")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.ApiAssembly == apiAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        failures.Should().BeEmpty(
            "API controllers should end with 'Controller'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }
}