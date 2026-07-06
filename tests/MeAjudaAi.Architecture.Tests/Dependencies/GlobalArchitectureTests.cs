using MeAjudaAi.Architecture.Tests.Helpers;
using MeAjudaAi.Architecture.Tests.Helpers.Models;

namespace MeAjudaAi.Architecture.Tests.Dependencies;

/// <summary>
/// Testes globais de arquitetura seguindo as recomendações de Milan Jovanovic
/// Estes testes garantem que os limites arquiteturais sejam mantidos em toda a solução
/// </summary>
public class GlobalArchitectureTests
{
    private static readonly IEnumerable<ModuleInfo> AllModules = ModuleDiscoveryHelper.DiscoverModules();

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.DomainAssembly == null || module.ApplicationAssembly == null) continue;

            var result = Types.InAssembly(module.DomainAssembly)
                .Should()
                .NotHaveDependencyOn(module.ApplicationAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Domain layer should not depend on Application layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.DomainAssembly == null || module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.DomainAssembly)
                .Should()
                .NotHaveDependencyOn(module.InfrastructureAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Domain layer should not depend on Infrastructure layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_API()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.DomainAssembly == null || module.ApiAssembly == null) continue;

            var result = Types.InAssembly(module.DomainAssembly)
                .Should()
                .NotHaveDependencyOn(module.ApiAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Domain layer should not depend on API layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.ApplicationAssembly == null || module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.ApplicationAssembly)
                .Should()
                .NotHaveDependencyOn(module.InfrastructureAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Application layer should not depend on Infrastructure layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_ShouldNotDependOn_API()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.ApplicationAssembly == null || module.ApiAssembly == null) continue;

            var result = Types.InAssembly(module.ApplicationAssembly)
                .Should()
                .NotHaveDependencyOn(module.ApiAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Application layer should not depend on API layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Controllers_ShouldNotDependOn_Infrastructure()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var module in AllModules)
        {
            if (module.ApiAssembly == null || module.InfrastructureAssembly == null) continue;

            var result = Types.InAssembly(module.ApiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .NotHaveDependencyOn(module.InfrastructureAssembly.GetName().Name)
                .GetResult();

            if (!result.IsSuccessful)
            {
                failures.AddRange(result.FailingTypes?.Select(t => $"{module.Name}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Controllers should not depend on Infrastructure layer directly. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Repositories_ShouldBeInInfrastructureLayer_AndFollowConventions()
    {
        // Arrange
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();

        // Act
        var (isValidNaming, namingViolations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            repositories,
            "Repository",
            "Repositories should end with 'Repository'");

        var repositoriesInWrongLayer = repositories
            .Where(repo => !repo.Namespace?.Contains(".Infrastructure") == true)
            .Select(repo => repo.FullName)
            .ToList();

        // Assert
        isValidNaming.Should().BeTrue(
            "All repositories should follow naming conventions. Violations: {0}",
            string.Join(", ", namingViolations));

        repositoriesInWrongLayer.Should().BeEmpty(
            "All repositories should be in Infrastructure layer. Violations: {0}",
            string.Join(", ", repositoriesInWrongLayer));
    }

    [Fact]
    public void AllServices_ShouldImplementInterfaces()
    {
        // Arrange
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies()
            .Concat(ModuleDiscoveryHelper.GetAllInfrastructureAssemblies());

        var services = ArchitecturalDiscoveryHelper.DiscoverTypesByConvention(
            allApplicationAssemblies,
            type => type.Name.EndsWith("Service") &&
                   type.IsClass &&
                   !type.IsAbstract &&
                   !type.IsInterface);

        // Act
        var servicesWithoutInterface = services
            .Where(service => service.GetInterfaces()
                .Count(i => !i.Namespace?.StartsWith("System") == true) == 0)
            .Select(service => service.FullName)
            .ToList();

        // Assert
        servicesWithoutInterface.Should().BeEmpty(
            "All services should implement at least one business interface. Violations: {0}",
            string.Join(", ", servicesWithoutInterface));
    }
}
