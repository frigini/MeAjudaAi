using FluentAssertions;
using MeAjudaAi.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.ServiceDefaults.Tests.Unit;

public class ExtensionsTests
{
    private const string LocalhostTelemetry = "http://localhost:4317";
    [Fact]
    public void AddServiceDefaults_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db",
                ["Telemetry:Endpoint"] = LocalhostTelemetry
            })
            .Build();
        
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        var result = Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockBuilder.Object);
    }

    [Fact]
    public void AddServiceDefaults_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Extensions.AddServiceDefaults(null!));
    }

    [Fact]
    public void AddServiceDefaults_ShouldConfigureLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(ILoggerFactory));
    }
}
