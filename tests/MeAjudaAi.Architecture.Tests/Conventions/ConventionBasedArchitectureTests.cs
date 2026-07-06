using MeAjudaAi.Architecture.Tests.Helpers;

namespace MeAjudaAi.Architecture.Tests.Conventions;

/// <summary>
/// Testes de convenção arquitetural usando discovery automático
/// Esta abordagem reduz o código boilerplate e torna os testes mais robustos
/// </summary>
public class ConventionBasedArchitectureTests
{
    [Fact]
    public void CommandHandlers_ShouldFollowNamingConventions()
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
            "All command handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void QueryHandlers_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Arrange
        var queryHandlers = ArchitecturalDiscoveryHelper.DiscoverQueryHandlers();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            queryHandlers,
            "Handler",
            "Query handlers should end with 'Handler'");

        // Assert
        isValid.Should().BeTrue(
            "All query handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void EventHandlers_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Arrange
        var eventHandlers = ArchitecturalDiscoveryHelper.DiscoverEventHandlers();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            eventHandlers,
            "Handler",
            "Event handlers should end with 'Handler'");

        // Assert
        isValid.Should().BeTrue(
            "All event handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void DomainEvents_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Arrange
        var domainEvents = ArchitecturalDiscoveryHelper.DiscoverDomainEvents();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            domainEvents,
            "DomainEvent",
            "Domain events should end with 'DomainEvent'");

        // Assert
        isValid.Should().BeTrue(
            "All domain events should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void Commands_DiscoveredByScrutor_ShouldFollowNamingConventions()
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
            "All commands should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void Queries_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Arrange
        var queries = ArchitecturalDiscoveryHelper.DiscoverQueries();

        // Act
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            queries,
            "Query",
            "Queries should end with 'Query'");

        // Assert
        isValid.Should().BeTrue(
            "All queries should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void Entities_DiscoveredByScrutor_ShouldFollowDomainConventions()
    {
        // Arrange
        var entities = ArchitecturalDiscoveryHelper.DiscoverEntities();

        // Act
        var entitiesInWrongLayer = entities
            .Where(entity => !entity.Namespace?.Contains(".Domain") == true)
            .Select(entity => entity.FullName)
            .ToList();

        // Assert
        entitiesInWrongLayer.Should().BeEmpty(
            "All entities should be in the Domain layer. Violations: {0}",
            string.Join(", ", entitiesInWrongLayer));
    }

    [Fact]
    public void Repositories_DiscoveredByScrutor_ShouldFollowInfrastructureConventions()
    {
        // Arrange
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();

        // Act
        var repositoriesInWrongLayer = repositories
            .Where(repo => !repo.Namespace?.Contains(".Infrastructure") == true)
            .Select(repo => repo.FullName)
            .ToList();

        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            repositories,
            "Repository",
            "Repositories should end with 'Repository'");

        // Assert
        repositoriesInWrongLayer.Should().BeEmpty(
            "All repositories should be in the Infrastructure layer. Violations: {0}",
            string.Join(", ", repositoriesInWrongLayer));

        isValid.Should().BeTrue(
            "All repositories should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void CustomConvention_AllServicesShouldFollowPattern()
    {
        // Arrange
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();
        var services = ArchitecturalDiscoveryHelper.DiscoverTypesByConvention(
            allApplicationAssemblies,
            type => type.Name.EndsWith("Service") &&
                   type.IsClass &&
                   !type.IsAbstract);

        // Act
        var servicesWithoutInterface = services
            .Where(service => service.GetInterfaces().Length == 0)
            .Select(service => service.FullName)
            .ToList();

        // Assert
        servicesWithoutInterface.Should().BeEmpty(
            "All services should implement at least one interface. Violations: {0}",
            string.Join(", ", servicesWithoutInterface));
    }

    [Fact]
    public void ScrutorDiscovery_ShouldWorkCorrectly()
    {
        // Arrange
        var commandHandlers = ArchitecturalDiscoveryHelper.DiscoverCommandHandlers();
        var queryHandlers = ArchitecturalDiscoveryHelper.DiscoverQueryHandlers();
        var eventHandlers = ArchitecturalDiscoveryHelper.DiscoverEventHandlers();
        var commands = ArchitecturalDiscoveryHelper.DiscoverCommands();
        var queries = ArchitecturalDiscoveryHelper.DiscoverQueries();

        // Act
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();

        // Assert
        commandHandlers.Should().NotBeNull("Scrutor should return valid collection for command handlers");
        queryHandlers.Should().NotBeNull("Scrutor should return valid collection for query handlers");
        eventHandlers.Should().NotBeNull("Scrutor should return valid collection for event handlers");
        commands.Should().NotBeNull("Scrutor should return valid collection for commands");
        queries.Should().NotBeNull("Scrutor should return valid collection for queries");
        repositories.Should().NotBeNull("Scrutor should return valid collection for repositories");
    }
}
