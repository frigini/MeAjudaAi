using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Global architecture tests following Milan Jovanovic's recommendations
/// These tests ensure architectural boundaries are maintained across the entire solution
/// </summary>
public class GlobalArchitectureTests
{
    // Assembly references for testing
    private static readonly Assembly DomainAssembly = typeof(MeAjudaAi.Modules.Users.Domain.Entities.User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(MeAjudaAi.Modules.Users.Application.Extensions).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(MeAjudaAi.Modules.Users.Infrastructure.Mappers.DomainEventMapperExtensions).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MeAjudaAi.Modules.Users.API.Mappers.RequestMapperExtensions).Assembly;
    private static readonly Assembly SharedAssembly = typeof(MeAjudaAi.Shared.Common.Result).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        // Domain layer should be completely independent
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Application layer. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        // Domain should never depend on Infrastructure
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Infrastructure layer. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_API()
    {
        // Domain should never depend on API/Controllers
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on API layer. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        // Application should only depend on abstractions, not concrete implementations
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure layer. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Application_ShouldNotDependOn_API()
    {
        // Application should not know about controllers/endpoints
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on API layer. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Controllers_ShouldNotDependOn_Infrastructure()
    {
        // Controllers should only depend on Application layer, not Infrastructure directly
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Controllers should not depend on Infrastructure layer directly. " +
            "Violations: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }
}