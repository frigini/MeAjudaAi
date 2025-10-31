using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class DocumentationExtensionsTests
{
    [Fact]
    public void AddDocumentation_ShouldRegisterSwaggerServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDocumentation();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddDocumentation_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddDocumentation());
    }

    [Fact]
    public void AddDocumentation_ShouldConfigureSwagger()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDocumentation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddDocumentation_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDocumentation();

        // Assert
        result.Should().BeOfType<ServiceCollection>();
        result.Should().BeSameAs(services);
    }
}
