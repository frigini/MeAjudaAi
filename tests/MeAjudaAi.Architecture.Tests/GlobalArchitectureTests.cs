using MeAjudaAi.Architecture.Tests.Helpers;

namespace MeAjudaAi.Architecture.Tests;

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
        // Camada Domain deve ser completamente independente
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Domain layer should not depend on Application layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        // Domain nunca deve depender de Infrastructure
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Domain layer should not depend on Infrastructure layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_API()
    {
        // Domain nunca deve depender de API/Controllers
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Domain layer should not depend on API layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        // Application deve depender apenas de abstrações, não de implementações concretas
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Application layer should not depend on Infrastructure layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_ShouldNotDependOn_API()
    {
        // Application não deve conhecer controllers/endpoints
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Application layer should not depend on API layer. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Controllers_ShouldNotDependOn_Infrastructure()
    {
        // Controllers devem depender apenas da Application layer, não diretamente de Infrastructure
        var failures = new List<string>();

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

        failures.Should().BeEmpty(
            "Controllers should not depend on Infrastructure layer directly. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Repositories_ShouldBeInInfrastructureLayer_AndFollowConventions()
    {
        // ✅ Discovery automático é ideal para encontrar implementações
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();

        // Validar convenção de nomenclatura
        var (isValidNaming, namingViolations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            repositories,
            "Repository",
            "Repositories should end with 'Repository'");

        isValidNaming.Should().BeTrue(
            "All repositories should follow naming conventions. Violations: {0}",
            string.Join(", ", namingViolations));

        // Validar que estão na camada correta
        var repositoriesInWrongLayer = repositories
            .Where(repo => !repo.Namespace?.Contains(".Infrastructure") == true)
            .Select(repo => repo.FullName)
            .ToList();

        repositoriesInWrongLayer.Should().BeEmpty(
            "All repositories should be in Infrastructure layer. Violations: {0}",
            string.Join(", ", repositoriesInWrongLayer));

        Console.WriteLine($"✅ Validated {repositories.Count()} repositories");
    }

    [Fact]
    public void AllServices_ShouldImplementInterfaces()
    {
        // ✅ Discovery automático é ideal para validações customizadas
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies()
            .Concat(ModuleDiscoveryHelper.GetAllInfrastructureAssemblies());

        var services = ArchitecturalDiscoveryHelper.DiscoverTypesByConvention(
            allApplicationAssemblies,
            type => type.Name.EndsWith("Service") &&
                   type.IsClass &&
                   !type.IsAbstract &&
                   !type.IsInterface);

        // Validar que todos os services implementam pelo menos uma interface
        var servicesWithoutInterface = services
            .Where(service => service.GetInterfaces()
                .Where(i => !i.Namespace?.StartsWith("System") == true)
                .Count() == 0)
            .Select(service => service.FullName)
            .ToList();

        servicesWithoutInterface.Should().BeEmpty(
            "All services should implement at least one business interface. Violations: {0}",
            string.Join(", ", servicesWithoutInterface));

        Console.WriteLine($"✅ Validated {services.Count()} services");
    }
}
