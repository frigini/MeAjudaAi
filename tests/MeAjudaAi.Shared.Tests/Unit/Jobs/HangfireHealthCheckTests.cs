using FluentAssertions;
using Hangfire;
using MeAjudaAi.Shared.Jobs.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Jobs;

/// <summary>
/// Testes unitários para HangfireHealthCheck
/// Verifica detecção de status do Hangfire (Healthy/Degraded)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Hangfire")]
public class HangfireHealthCheckTests
{
    private readonly Mock<ILogger<HangfireHealthCheck>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IBackgroundJobClient> _jobClientMock;

    public HangfireHealthCheckTests()
    {
        _loggerMock = new Mock<ILogger<HangfireHealthCheck>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _jobClientMock = new Mock<IBackgroundJobClient>();
    }

    [Fact]
    public async Task CheckHealthAsync_WithConfiguredHangfire_ShouldReturnHealthy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHangfire(config => config.UseInMemoryStorage());
        services.AddHangfireServer();
        var serviceProvider = services.BuildServiceProvider();

        var jobClient = serviceProvider.GetService<IBackgroundJobClient>();
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobClient)))
            .Returns(jobClient);

        var healthCheck = new HangfireHealthCheck(_loggerMock.Object, _serviceProviderMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy,
            "Hangfire with configured storage and job client should be healthy");
        result.Description.Should().Contain("operational");
        result.Data.Should().ContainKey("storage_type");
        result.Data.Should().ContainKey("job_client_available");
        result.Data["job_client_available"].Should().Be(true);
        
        // Cleanup
        await serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task CheckHealthAsync_WithoutJobClient_ShouldReturnDegraded()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHangfire(config => config.UseInMemoryStorage());
        var serviceProvider = services.BuildServiceProvider();

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobClient)))
            .Returns((IBackgroundJobClient?)null);

        var healthCheck = new HangfireHealthCheck(_loggerMock.Object, _serviceProviderMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded,
            "Hangfire without job client should be degraded");
        result.Description.Should().Contain("unavailable");
        result.Data.Should().ContainKey("job_client_available");
        result.Data["job_client_available"].Should().Be(false);
        
        // Cleanup
        await serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task CheckHealthAsync_WithoutHangfireInitialized_ShouldReturnDegraded()
    {
        // Arrange
        // Service provider sem IBackgroundJobClient e sem JobStorage
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobClient)))
            .Returns((IBackgroundJobClient?)null);
        
        var healthCheck = new HangfireHealthCheck(_loggerMock.Object, _serviceProviderMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        // Sem JobStorage configurado, deve retornar Degraded
        result.Status.Should().Be(HealthStatus.Degraded,
            "Hangfire not initialized should be degraded");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeTimestampInData()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHangfire(config => config.UseInMemoryStorage());
        services.AddHangfireServer();
        var serviceProvider = services.BuildServiceProvider();

        var jobClient = serviceProvider.GetService<IBackgroundJobClient>();
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobClient)))
            .Returns(jobClient);

        var healthCheck = new HangfireHealthCheck(_loggerMock.Object, _serviceProviderMock.Object);

        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        var afterCheck = DateTime.UtcNow;

        // Assert
        result.Data.Should().ContainKey("timestamp");
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeOnOrAfter(beforeCheck).And.BeOnOrBefore(afterCheck);
        
        // Cleanup
        await serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeComponentIdentifier()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHangfire(config => config.UseInMemoryStorage());
        services.AddHangfireServer();
        var serviceProvider = services.BuildServiceProvider();

        var jobClient = serviceProvider.GetService<IBackgroundJobClient>();
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobClient)))
            .Returns(jobClient);

        var healthCheck = new HangfireHealthCheck(_loggerMock.Object, _serviceProviderMock.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Data.Should().ContainKey("component");
        result.Data["component"].Should().Be("hangfire");
        
        // Cleanup
        await serviceProvider.DisposeAsync();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HangfireHealthCheck(null!, _serviceProviderMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new HangfireHealthCheck(_loggerMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }
}
