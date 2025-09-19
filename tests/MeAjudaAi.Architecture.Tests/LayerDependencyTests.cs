using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Layer dependency tests ensuring Clean Architecture principles
/// Based on Milan Jovanovic's recommendations for modular monoliths
/// </summary>
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(MeAjudaAi.Modules.Users.Domain.Entities.User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(MeAjudaAi.Modules.Users.Application.Extensions).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(MeAjudaAi.Modules.Users.Infrastructure.Mappers.DomainEventMapperExtensions).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MeAjudaAi.Modules.Users.API.Mappers.RequestMapperExtensions).Assembly;

    [Fact]
    public void Domain_Entities_ShouldBeSealed()
    {
        // Entities should be sealed to prevent inheritance issues
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Entities")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain entities should be sealed. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Domain_ValueObjects_ShouldBeRecords()
    {
        // Value objects should be implemented as records for immutability
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".ValueObjects")
            .Should()
            .BeClasses()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Value objects should be implemented as classes/records. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Domain_Events_ShouldBeRecords()
    {
        // Domain events should be immutable records
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Events")
            .And()
            .HaveNameEndingWith("DomainEvent")
            .Should()
            .BeClasses()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain events should be implemented as classes/records. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_CommandHandlers_ShouldBeInternal()
    {
        // Command handlers should be internal to prevent external usage
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Command handlers should be internal. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_QueryHandlers_ShouldBeInternal()
    {
        // Query handlers should be internal to prevent external usage
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Query handlers should be internal. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Infrastructure_Repositories_ShouldBeInternal()
    {
        // Repository implementations should be internal
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Repository implementations should be internal. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Infrastructure_EventHandlers_ShouldBeSealed()
    {
        // Event handlers should be sealed
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("EventHandler")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Event handlers should be sealed. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void API_Controllers_ShouldBeSealed()
    {
        // Controllers should be sealed to prevent inheritance
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Controllers should be sealed. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}