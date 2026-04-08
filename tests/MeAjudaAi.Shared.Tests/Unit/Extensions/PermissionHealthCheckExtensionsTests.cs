using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
public class PermissionHealthCheckExtensionsTests
{
    [Fact]
    public void AddPermissionSystemHealthCheck_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddPermissionSystemHealthCheck());
    }

    [Fact]
    public void AddPermissionSystemHealthCheck_WithValidServices_ShouldAddHealthCheck()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPermissionSystemHealthCheck();

        // Assert
        result.Should().BeSameAs(services);
        var hasHealthCheck = services.Any(d => 
            d.ServiceType.Name.Contains("HealthCheck") || 
            d.ImplementationType?.Name.Contains("Permission") == true);
            
        hasHealthCheck.Should().BeTrue();
    }
}
