using FluentAssertions;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.Unit.Commands;

/// <summary>
/// Testes para Commands.Extensions - registro do CommandDispatcher no DI container
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Commands")]
public class CommandsExtensionsTests
{
    [Fact]
    public void AddCommands_ShouldRegisterCommandDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommands();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dispatcher = serviceProvider.GetService<ICommandDispatcher>();
        dispatcher.Should().NotBeNull("CommandDispatcher should be registered");
        dispatcher.Should().BeOfType<CommandDispatcher>();
    }

    [Fact]
    public void AddCommands_ShouldRegisterAsScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommands();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandDispatcher));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped,
            "CommandDispatcher should be registered with Scoped lifetime");
    }

    [Fact]
    public void AddCommands_CalledMultipleTimes_ShouldRegisterMultipleInstances()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommands();
        services.AddCommands(); // Segunda chamada

        // Assert
        var descriptors = services.Where(d => d.ServiceType == typeof(ICommandDispatcher)).ToList();
        descriptors.Should().HaveCount(2,
            "calling AddCommands multiple times should register multiple instances");
    }

    [Fact]
    public void AddCommands_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCommands();

        // Assert
        result.Should().BeSameAs(services, "should return the same IServiceCollection for fluent chaining");
    }

    [Fact]
    public void AddCommands_WithExistingServices_ShouldNotClearExistingRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        services.AddCommands();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(ITestService),
            "existing service registrations should be preserved");
        services.Should().Contain(d => d.ServiceType == typeof(ICommandDispatcher),
            "CommandDispatcher should be added");
    }

    [Fact]
    public void AddCommands_ResolvedDispatcher_ShouldBeUsable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCommands();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();

        // Assert
        dispatcher.Should().NotBeNull();
        dispatcher.Should().BeAssignableTo<ICommandDispatcher>();
    }

    [Fact]
    public void AddCommands_MultipleScopes_ShouldCreateDifferentInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCommands();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        ICommandDispatcher? dispatcher1;
        ICommandDispatcher? dispatcher2;

        using (var scope1 = serviceProvider.CreateScope())
        {
            dispatcher1 = scope1.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            dispatcher2 = scope2.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        }

        // Assert
        dispatcher1.Should().NotBeSameAs(dispatcher2,
            "scoped services should create different instances per scope");
    }

    [Fact]
    public void AddCommands_WithinSameScope_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCommands();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var dispatcher1 = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var dispatcher2 = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        // Assert
        dispatcher1.Should().BeSameAs(dispatcher2,
            "scoped services should return the same instance within the same scope");
    }

    // Helper test interface and implementation
    private interface ITestService { }
    private class TestService : ITestService { }
}
