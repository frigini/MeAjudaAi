using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
public class ValidationExtensionsTests
{
    [Fact]
    public void AddValidation_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddValidation());
    }

    [Fact]
    public void AddValidation_WithValidServices_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddValidation();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddValidation_ShouldRegisterValidationBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidation();

        // Act
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MeAjudaAi.Shared.Mediator.IPipelineBehavior<,>));

        // Assert
        descriptor.Should().NotBeNull();
    }
}
