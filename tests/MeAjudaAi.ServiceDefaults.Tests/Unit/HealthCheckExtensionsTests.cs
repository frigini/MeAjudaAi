using FluentAssertions;
using MeAjudaAi.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MeAjudaAi.ServiceDefaults.Tests.Unit;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceDefaults")]
[Trait("Layer", "ServiceDefaults")]
public class HealthCheckExtensionsTests
{
    [Fact]
    public void AddDefaultHealthChecks_ShouldRegisterHealthCheckServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        var result = HealthCheckExtensions.AddDefaultHealthChecks(mockBuilder.Object);

        // Assert
        result.Should().NotBeNull();
        services.Should().Contain(s => s.ServiceType == typeof(HealthCheckService));
    }

    [Fact]
    public void AddDefaultHealthChecks_WithGenericBuilder_ShouldReturnSameBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        var result = HealthCheckExtensions.AddDefaultHealthChecks(mockBuilder.Object);

        // Assert
        result.Should().BeSameAs(mockBuilder.Object);
    }

    [Fact]
    public void AddDefaultHealthChecks_ShouldAddSelfHealthCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        HealthCheckExtensions.AddDefaultHealthChecks(mockBuilder.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        healthCheckService.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    public void AddDefaultHealthChecks_WithNullBuilder_ShouldThrowArgumentNullException(IHostApplicationBuilder builder)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HealthCheckExtensions.AddDefaultHealthChecks(builder));
    }
}
