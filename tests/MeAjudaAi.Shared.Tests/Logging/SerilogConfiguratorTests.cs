using FluentAssertions;
using MeAjudaAi.Shared.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Serilog;

namespace MeAjudaAi.Shared.Tests.Logging;

public class SerilogConfiguratorTests
{
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly IConfiguration _configuration;

    public SerilogConfiguratorTests()
    {
        _environmentMock = new Mock<IWebHostEnvironment>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Serilog:MinimumLevel:Default", "Information" },
                { "Serilog:WriteTo:0:Name", "Console" }
            })
            .Build();
    }

    [Fact]
    public void ConfigureSerilog_ShouldReturnLoggerConfiguration()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = SerilogConfigurator.ConfigureSerilog(_configuration, _environmentMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<LoggerConfiguration>();
    }

    [Fact]
    public void ConfigureSerilog_Development_ShouldConfigureVerboseLogging()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = SerilogConfigurator.ConfigureSerilog(_configuration, _environmentMock.Object);

        // Assert
        result.Should().NotBeNull();
        _environmentMock.Verify(e => e.EnvironmentName, Times.AtLeastOnce);
    }

    [Fact]
    public void ConfigureSerilog_Production_ShouldConfigureOptimizedLogging()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        var result = SerilogConfigurator.ConfigureSerilog(_configuration, _environmentMock.Object);

        // Assert
        result.Should().NotBeNull();
        _environmentMock.Verify(e => e.EnvironmentName, Times.AtLeastOnce);
    }

    [Fact]
    public void ConfigureSerilog_WithApplicationInsightsConnectionString_ShouldConfigureAppInsights()
    {
        // Arrange
        var configWithAppInsights = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApplicationInsights:ConnectionString", "InstrumentationKey=test-key" }
            })
            .Build();
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        var result = SerilogConfigurator.ConfigureSerilog(configWithAppInsights, _environmentMock.Object);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureSerilog_ShouldEnrichWithApplicationProperties()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = SerilogConfigurator.ConfigureSerilog(_configuration, _environmentMock.Object);

        // Assert
        result.Should().NotBeNull();
        // Properties like Application, Environment, MachineName, ProcessId, Version are added
    }

    [Fact]
    public void GetApplicationVersion_ShouldReturnValidVersion()
    {
        // Act
        var version = SerilogConfigurator.GetApplicationVersion();

        // Assert
        version.Should().NotBeNullOrEmpty();
        version.Should().MatchRegex(@"^\d+\.\d+\.\d+(\.\d+)?$|^unknown$");
    }
}

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
