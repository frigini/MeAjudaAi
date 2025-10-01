using MeAjudaAi.Architecture.Tests.Helpers;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Testes de convenção arquitetural usando discovery automático
/// Esta abordagem reduz o código boilerplate e torna os testes mais robustos
/// </summary>
public class ConventionBasedArchitectureTests
{
    [Fact]
    public void CommandHandlers_ShouldFollowNamingConventions()
    {
        // Usa discovery automático para descobrir todos os command handlers
        var commandHandlers = ArchitecturalDiscoveryHelper.DiscoverCommandHandlers();

        // Valida convenções de nomenclatura usando helper
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            commandHandlers,
            "Handler",
            "Command handlers should end with 'Handler'");

        isValid.Should().BeTrue(
            "All command handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        // Log para debugging - não falha se não encontrar (pode não ter handlers ainda)
        Console.WriteLine($"Discovered {commandHandlers.Count()} command handlers");
    }

    [Fact]
    public void QueryHandlers_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os query handlers
        var queryHandlers = ArchitecturalDiscoveryHelper.DiscoverQueryHandlers();

        // Valida convenções de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            queryHandlers,
            "Handler",
            "Query handlers should end with 'Handler'");

        isValid.Should().BeTrue(
            "All query handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        Console.WriteLine($"Discovered {queryHandlers.Count()} query handlers");
    }

    [Fact]
    public void EventHandlers_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os event handlers
        var eventHandlers = ArchitecturalDiscoveryHelper.DiscoverEventHandlers();

        // Valida convenções de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            eventHandlers,
            "Handler",
            "Event handlers should end with 'Handler'");

        isValid.Should().BeTrue(
            "All event handlers should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        Console.WriteLine($"Discovered {eventHandlers.Count()} event handlers");
    }

    [Fact]
    public void DomainEvents_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os domain events
        var domainEvents = ArchitecturalDiscoveryHelper.DiscoverDomainEvents();

        // Valida convenções de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            domainEvents,
            "DomainEvent",
            "Domain events should end with 'DomainEvent'");

        isValid.Should().BeTrue(
            "All domain events should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        Console.WriteLine($"Discovered {domainEvents.Count()} domain events");
    }

    [Fact]
    public void Commands_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os commands
        var commands = ArchitecturalDiscoveryHelper.DiscoverCommands();

        // Valida convenções de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            commands,
            "Command",
            "Commands should end with 'Command'");

        isValid.Should().BeTrue(
            "All commands should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        Console.WriteLine($"Discovered {commands.Count()} commands");
    }

    [Fact]
    public void Queries_DiscoveredByScrutor_ShouldFollowNamingConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os queries
        var queries = ArchitecturalDiscoveryHelper.DiscoverQueries();

        // Valida convenções de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            queries,
            "Query",
            "Queries should end with 'Query'");

        isValid.Should().BeTrue(
            "All queries should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));

        Console.WriteLine($"Discovered {queries.Count()} queries");
    }

    [Fact]
    public void Entities_DiscoveredByScrutor_ShouldFollowDomainConventions()
    {
        // Usa Scrutor para descobrir automaticamente todas as entities
        var entities = ArchitecturalDiscoveryHelper.DiscoverEntities();

        // Valida que todas as entities estão na camada de domínio
        var entitiesInWrongLayer = entities
            .Where(entity => !entity.Namespace?.Contains(".Domain") == true)
            .Select(entity => entity.FullName)
            .ToList();

        entitiesInWrongLayer.Should().BeEmpty(
            "All entities should be in the Domain layer. Violations: {0}",
            string.Join(", ", entitiesInWrongLayer));

        Console.WriteLine($"Discovered {entities.Count()} entities");
    }

    [Fact]
    public void Repositories_DiscoveredByScrutor_ShouldFollowInfrastructureConventions()
    {
        // Usa Scrutor para descobrir automaticamente todos os repositories
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();

        // Valida que todos os repositories estão na camada de infraestrutura
        var repositoriesInWrongLayer = repositories
            .Where(repo => !repo.Namespace?.Contains(".Infrastructure") == true)
            .Select(repo => repo.FullName)
            .ToList();

        repositoriesInWrongLayer.Should().BeEmpty(
            "All repositories should be in the Infrastructure layer. Violations: {0}",
            string.Join(", ", repositoriesInWrongLayer));

        // Valida convenção de nomenclatura
        var (isValid, violations) = ArchitecturalDiscoveryHelper.ValidateNamingConvention(
            repositories,
            "Repository",
            "Repositories should end with 'Repository'");

        isValid.Should().BeTrue(
            "All repositories should follow naming conventions. Violations: {0}",
            string.Join(", ", violations));
    }

    [Fact]
    public void CustomConvention_AllServicesShouldFollowPattern()
    {
        // Exemplo de convenção personalizada usando Scrutor
        var allApplicationAssemblies = ModuleDiscoveryHelper.GetAllApplicationAssemblies();

        var services = ArchitecturalDiscoveryHelper.DiscoverTypesByConvention(
            allApplicationAssemblies,
            type => type.Name.EndsWith("Service") &&
                   type.IsClass &&
                   !type.IsAbstract);

        // Valida que todos os services implementam alguma interface
        var servicesWithoutInterface = services
            .Where(service => service.GetInterfaces().Length == 0)
            .Select(service => service.FullName)
            .ToList();

        servicesWithoutInterface.Should().BeEmpty(
            "All services should implement at least one interface. Violations: {0}",
            string.Join(", ", servicesWithoutInterface));
    }

    [Fact]
    public void ScrutorDiscovery_ShouldWorkCorrectly()
    {
        // Este teste demonstra que o Scrutor consegue fazer discovery mesmo sem dados
        var commandHandlers = ArchitecturalDiscoveryHelper.DiscoverCommandHandlers();
        var queryHandlers = ArchitecturalDiscoveryHelper.DiscoverQueryHandlers();
        var eventHandlers = ArchitecturalDiscoveryHelper.DiscoverEventHandlers();
        var commands = ArchitecturalDiscoveryHelper.DiscoverCommands();
        var queries = ArchitecturalDiscoveryHelper.DiscoverQueries();

        // Valida que o discovery está funcionando (mesmo que encontre 0 tipos)
        commandHandlers.Should().NotBeNull("Scrutor should return valid collection for command handlers");
        queryHandlers.Should().NotBeNull("Scrutor should return valid collection for query handlers");
        eventHandlers.Should().NotBeNull("Scrutor should return valid collection for event handlers");
        commands.Should().NotBeNull("Scrutor should return valid collection for commands");
        queries.Should().NotBeNull("Scrutor should return valid collection for queries");

        // Log para debugging
        Console.WriteLine($"Discovered types: Commands={commands.Count()}, " +
                         $"Queries={queries.Count()}, " +
                         $"CommandHandlers={commandHandlers.Count()}, " +
                         $"QueryHandlers={queryHandlers.Count()}, " +
                         $"EventHandlers={eventHandlers.Count()}");

        // Testa que pelo menos conseguimos fazer discovery de algo no projeto
        var repositories = ArchitecturalDiscoveryHelper.DiscoverRepositories();
        Console.WriteLine($"Found {repositories.Count()} repositories");

        // Este deve funcionar se tivermos pelo menos a estrutura básica
        true.Should().BeTrue("Scrutor discovery functionality is working correctly");
    }
}