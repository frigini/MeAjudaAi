using FluentAssertions;
using MeAjudaAi.Shared.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MeAjudaAi.Shared.Tests.Logging;

public class LoggingConfigurationExtensionsTests
{
    [Fact]
    public void AddStructuredLogging_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = services.AddStructuredLogging(configuration, environment.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddStructuredLogging_ShouldRegisterSerilogServices()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        services.AddStructuredLogging(configuration, environment.Object);

        // Assert
        services.Should().Contain(s => s.ServiceType.Name.Contains("ILogger"));
    }
}
