using MeAjudaAi.Architecture.Tests.Helpers;
using MeAjudaAi.Architecture.Tests.Helpers.Models;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests.Conventions;

/// <summary>
/// Testes de convenção de nomes para garantir consistência em toda a solução
/// Garante padrões de codificação e manutenibilidade
/// </summary>
public class NamingConventionTests
{
    private static readonly IEnumerable<ModuleInfo> AllModules = ModuleDiscoveryHelper.DiscoverModules();
    private static readonly IEnumerable<Assembly> AllDomainAssemblies = ModuleDiscoveryHelper.GetAllDomainAssemblies();
    private static readonly IEnumerable<Assembly> AllApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();
    private static readonly IEnumerable<Assembly> AllInfrastructureAssemblies = ModuleDiscoveryHelper.GetAllInfrastructureAssemblies();
    private static readonly IEnumerable<Assembly> AllApiAssemblies = ModuleDiscoveryHelper.GetAllApiAssemblies();
    private static readonly Assembly SharedAssembly = typeof(MeAjudaAi.Contracts.Functional.Result).Assembly;

    [Fact]
    public void Domain_Events_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Events")
                .And()
                .ImplementInterface(typeof(MeAjudaAi.Shared.Events.IDomainEvent))
                .Should()
                .HaveNameEndingWith("DomainEvent")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Os eventos de domínio devem terminar com 'DomainEvent'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Integration_Events_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var result = Types.InAssembly(SharedAssembly)
            .That()
            .ResideInNamespaceContaining(".Messages")
            .And()
            .Inherit(typeof(MeAjudaAi.Shared.Events.IntegrationEvent))
            .Should()
            .HaveNameEndingWith("IntegrationEvent")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Os eventos de integração devem terminar com 'IntegrationEvent'. " +
            "Violations: {0}",
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_Commands_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var applicationAssembly in AllApplicationAssemblies)
        {
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Commands")
                .And()
                .ImplementInterface(typeof(MeAjudaAi.Shared.Commands.ICommand))
                .Should()
                .HaveNameEndingWith("Command")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.ApplicationAssembly == applicationAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Os comandos devem terminar com 'Command'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Application_Queries_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var applicationAssembly in AllApplicationAssemblies)
        {
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Queries")
                .And()
                .ImplementInterface(typeof(MeAjudaAi.Shared.Queries.IQuery<>))
                .Should()
                .HaveNameEndingWith("Query")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.ApplicationAssembly == applicationAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "As consultas devem terminar com 'Query'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Infrastructure_Repositories_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var failures = new List<string>();

        // Act
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

        // Assert
        failures.Should().BeEmpty(
            "As implementações do repositório devem terminar com 'Repository'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Domain_Interfaces_ShouldStartWithI()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                failures.AddRange(result.FailingTypes?.Select(t => $"{moduleName}: {t.FullName}") ?? []);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "As interfaces devem começar com 'I'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Value_Objects_ShouldNotHaveIdSuffix()
    {
        // Arrange
        var failures = new List<string>();
        var allowedIdTypes = new[] { "UserId", "Email", "DocumentId", "ProviderId", "ReviewId", "SearchableProviderId", "ServiceCategoryId", "ServiceId" };

        // Act
        foreach (var domainAssembly in AllDomainAssemblies)
        {
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespaceEndingWith(".ValueObjects")
                .Should()
                .NotHaveNameEndingWith("Id")
                .GetResult();

            if (!result.IsSuccessful)
            {
                var moduleName = AllModules
                    .FirstOrDefault(m => m.DomainAssembly == domainAssembly)?.Name ?? "Unknown";

                var actualViolations = result.FailingTypes?
                    .Where(t => !allowedIdTypes.Contains(t.Name))
                    .Select(t => $"{moduleName}: {t.FullName}")
                    .ToList() ?? [];

                failures.AddRange(actualViolations);
            }
        }

        // Assert
        failures.Should().BeEmpty(
            "Os objetos de valor não devem terminar com 'Id' (exceto tipos de ID específicos). " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void API_Controllers_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var failures = new List<string>();

        // Act
        foreach (var apiAssembly in AllApiAssemblies)
        {
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespaceEndingWith(".Controllers")
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

        // Assert
        failures.Should().BeEmpty(
            "Os controladores devem terminar com 'Controller'. " +
            "Violations: {0}",
            string.Join(", ", failures));
    }

    [Fact]
    public void Exception_Classes_ShouldHaveCorrectSuffix()
    {
        // Arrange
        var allAssemblies = AllDomainAssemblies
            .Concat(AllApplicationAssemblies)
            .Concat(AllInfrastructureAssemblies)
            .Append(SharedAssembly)
            .ToArray();

        // Act
        var result = Types.InAssemblies(allAssemblies)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "As classes de exceção devem terminar com 'Exception'. " +
            "Violations: {0}",
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    #region Discovery-Based Tests (Alternative approach demonstrating automated discovery)

    /// <summary>
    /// Demonstra como usar discovery automático para simplificar o discovery de Command Handlers
    /// Esta abordagem é mais concisa que o NetArchTest tradicional
    /// </summary>
    [Fact]
    public void DiscoveryBased_CommandHandlers_ShouldFollowNamingConventions()
    {
        // Arrange
        var commandHandlers = ArchitecturalDiscoveryHelper.DiscoverCommandHandlers();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            commandHandlers,
            "Handler",
            "Command handlers should end with 'Handler'");

        // Assert
        isValid.Should().BeTrue(
            "Command handlers discovered automatically should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    /// <summary>
    /// Demonstra como usar discovery automático para simplificar o discovery de Commands
    /// Compara com a abordagem tradicional acima
    /// </summary>
    [Fact]
    public void DiscoveryBased_Commands_ShouldFollowNamingConventions()
    {
        // Arrange
        var commands = ArchitecturalDiscoveryHelper.DiscoverCommands();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            commands,
            "Command",
            "Commands should end with 'Command'");

        // Assert
        isValid.Should().BeTrue(
            "Commands discovered automatically should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    /// <summary>
    /// Demonstra como usar discovery automático para descoberta personalizada baseada em convenções
    /// Exemplo: descobrir todos os tipos que seguem padrão específico
    /// </summary>
    [Fact]
    public void DiscoveryBased_CustomPatternDiscovery_ShouldWork()
    {
        // Arrange
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();
        var validators = ArchitecturalDiscoveryHelper.DiscoverTypesByConvention(
            allApplicationAssemblies,
            type => type.Name.EndsWith("Validator") &&
                   type.IsClass &&
                   !type.IsAbstract);

        // Act
        var validatorsInWrongNamespace = validators
            .Where(validator =>
                !validator.Namespace?.Contains("Validators") == true &&
                !validator.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Options.IValidateOptions<>)))
            .Select(validator => validator.FullName)
            .ToList();

        // Assert
        validatorsInWrongNamespace.Should().BeEmpty(
            "Validators should be in 'Validators' namespace. Violations: {0}",
            string.Join(", ", validatorsInWrongNamespace));
    }

    #endregion
}
