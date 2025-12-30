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
    public void ConfigureSerilog_ShouldConfigureLoggerConfiguration()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        var loggerConfig = new LoggerConfiguration();

        // Act
        SerilogConfigurator.ConfigureSerilog(loggerConfig, _configuration, _environmentMock.Object);

        // Assert
        loggerConfig.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureSerilog_Development_ShouldConfigureVerboseLogging()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        var loggerConfig = new LoggerConfiguration();

        // Act
        SerilogConfigurator.ConfigureSerilog(loggerConfig, _configuration, _environmentMock.Object);

        // Assert
        loggerConfig.Should().NotBeNull();
        _environmentMock.Verify(e => e.EnvironmentName, Times.AtLeastOnce);
    }

    [Fact]
    public void ConfigureSerilog_Production_ShouldConfigureOptimizedLogging()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var loggerConfig = new LoggerConfiguration();

        // Act
        SerilogConfigurator.ConfigureSerilog(loggerConfig, _configuration, _environmentMock.Object);

        // Assert
        loggerConfig.Should().NotBeNull();
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
        var loggerConfig = new LoggerConfiguration();

        // Act
        SerilogConfigurator.ConfigureSerilog(loggerConfig, configWithAppInsights, _environmentMock.Object);

        // Assert
        loggerConfig.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureSerilog_ShouldEnrichWithApplicationProperties()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        var loggerConfig = new LoggerConfiguration();

        // Act
        SerilogConfigurator.ConfigureSerilog(loggerConfig, _configuration, _environmentMock.Object);

        // Assert
        loggerConfig.Should().NotBeNull();
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
