using MeAjudaAi.Modules.Communications.Application.Handlers;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using NetArchTest.Rules;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Architecture;

public class CommunicationsArchitectureTests
{
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(UserRegisteredIntegrationEventHandler).Assembly;
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(EmailTemplate).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(CommunicationsDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Layers()
    {
        Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Communications.Application")
            .GetResult()
            .IsSuccessful.Should().BeTrue("Domain should not depend on Application");

        Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Communications.Infrastructure")
            .GetResult()
            .IsSuccessful.Should().BeTrue("Domain should not depend on Infrastructure");
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Communications.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependency_On_Application()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("MeAjudaAi.Modules.Communications.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Repositories_Should_Be_Internal()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
