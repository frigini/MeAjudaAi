using FluentAssertions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Unit.Queries;

/// <summary>
/// Testes para Queries.Extensions - registro do QueryDispatcher no DI container
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Queries")]
public class QueriesExtensionsTests
{
    [Fact]
    public void AddQueries_ShouldRegisterQueryDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueries();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<IQueryDispatcher>();
        dispatcher.Should().NotBeNull("QueryDispatcher should be registered");
        dispatcher.Should().BeOfType<QueryDispatcher>();
    }

    [Fact]
    public void AddQueries_ShouldRegisterAsScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueries();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IQueryDispatcher));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped,
            "QueryDispatcher should be registered with Scoped lifetime");
    }

    [Fact]
    public void AddQueries_CalledMultipleTimes_ShouldRegisterMultipleInstances()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddQueries();
        services.AddQueries(); // Segunda chamada

        // Assert
        var descriptors = services.Where(d => d.ServiceType == typeof(IQueryDispatcher)).ToList();
        descriptors.Should().HaveCount(2,
            "calling AddQueries multiple times should register multiple instances");
    }

    [Fact]
    public void AddQueries_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddQueries();

        // Assert
        result.Should().BeSameAs(services, "should return the same IServiceCollection for fluent chaining");
    }

    [Fact]
    public void AddQueries_WithExistingServices_ShouldNotClearExistingRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        services.AddQueries();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(ITestService),
            "existing service registrations should be preserved");
        services.Should().Contain(d => d.ServiceType == typeof(IQueryDispatcher),
            "QueryDispatcher should be added");
    }

    [Fact]
    public void AddQueries_ResolvedDispatcher_ShouldBeUsable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddQueries();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var dispatcher = serviceProvider.GetRequiredService<IQueryDispatcher>();

        // Assert
        dispatcher.Should().NotBeNull();
        dispatcher.Should().BeAssignableTo<IQueryDispatcher>();
    }

    [Fact]
    public void AddQueries_MultipleScopes_ShouldCreateDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddQueries();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        IQueryDispatcher? dispatcher1;
        IQueryDispatcher? dispatcher2;

        using (var scope1 = serviceProvider.CreateScope())
        {
            dispatcher1 = scope1.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            dispatcher2 = scope2.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        }

        // Assert
        dispatcher1.Should().NotBeSameAs(dispatcher2,
            "scoped services should create different instances per scope");
    }

    [Fact]
    public void AddQueries_WithinSameScope_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddQueries();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var dispatcher1 = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        var dispatcher2 = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();

        // Assert
        dispatcher1.Should().BeSameAs(dispatcher2,
            "scoped services should return the same instance within the same scope");
    }

    // Helper test interface and implementation
    private interface ITestService { }
    private class TestService : ITestService { }
}
