using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
public class ModuleServiceRegistrationExtensionsTests
{
    [Fact]
    public void AddModuleServices_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddModuleServices());
    }

    [Fact]
    public void AddModuleServices_WithValidAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw even with no matching types
        var result = services.AddModuleServices();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddModuleRepositories_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddModuleRepositories());
    }

    [Fact]
    public void AddModuleRepositories_WithValidAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddModuleRepositories();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddModuleValidators_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddModuleValidators());
    }

    [Fact]
    public void AddModuleValidators_WithValidAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddModuleValidators();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddModuleCacheServices_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddModuleCacheServices());
    }

    [Fact]
    public void AddModuleCacheServices_WithValidAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddModuleCacheServices();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddModuleDomainServices_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddModuleDomainServices());
    }

    [Fact]
    public void AddModuleDomainServices_WithValidAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddModuleDomainServices();
        result.Should().BeSameAs(services);
    }
}
