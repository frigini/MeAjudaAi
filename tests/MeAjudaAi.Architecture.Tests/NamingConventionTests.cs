using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Naming convention tests to ensure consistency across the solution
/// Enforces coding standards and maintainability
/// </summary>
public class NamingConventionTests
{
    private static readonly Assembly DomainAssembly = typeof(MeAjudaAi.Modules.Users.Domain.Entities.User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(MeAjudaAi.Modules.Users.Application.Extensions).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(MeAjudaAi.Modules.Users.Infrastructure.Mappers.DomainEventMapperExtensions).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MeAjudaAi.Modules.Users.API.Mappers.RequestMapperExtensions).Assembly;
    private static readonly Assembly SharedAssembly = typeof(MeAjudaAi.Shared.Functional.Result).Assembly;

    [Fact]
    public void Domain_Events_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Events")
            .And()
            .ImplementInterface(typeof(MeAjudaAi.Shared.Events.IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain events should end with 'DomainEvent'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Integration_Events_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(SharedAssembly)
            .That()
            .ResideInNamespaceContaining(".Messages")
            .And()
            .Inherit(typeof(MeAjudaAi.Shared.Events.IntegrationEvent))
            .Should()
            .HaveNameEndingWith("IntegrationEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Integration events should end with 'IntegrationEvent'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_Commands_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Commands")
            .And()
            .ImplementInterface(typeof(MeAjudaAi.Shared.Commands.ICommand))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Commands should end with 'Command'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_Queries_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Queries")
            .And()
            .ImplementInterface(typeof(MeAjudaAi.Shared.Queries.IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Queries should end with 'Query'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Infrastructure_Repositories_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Repositories")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Repository implementations should end with 'Repository'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Domain_Interfaces_ShouldStartWithI()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Interfaces should start with 'I'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Value_Objects_ShouldNotHaveIdSuffix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".ValueObjects")
            .Should()
            .NotHaveNameEndingWith("Id")
            .GetResult();

        // Allow specific ID value objects like UserId, Email, etc.
        var allowedIdTypes = new[] { "UserId", "Email" };
        var actualViolations = result.FailingTypes?
            .Where(t => !allowedIdTypes.Contains(t.Name))
            .ToList();

        (actualViolations?.Count ?? 0).Should().Be(0,
            "Value objects should not end with 'Id' (except specific ID types). " +
            "Violations: {0}", 
            string.Join(", ", actualViolations?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void API_Controllers_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Controllers")
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Controllers should end with 'Controller'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Exception_Classes_ShouldHaveCorrectSuffix()
    {
        var result = Types.InAssemblies([DomainAssembly, ApplicationAssembly, InfrastructureAssembly, SharedAssembly])
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Exception classes should end with 'Exception'. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}