using FluentAssertions;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

/// <summary>
/// Testes para DatabasePerformanceHealthCheck - verifica monitoramento de performance de database.
/// Cobre métricas configuradas, cenários de erro, validação de dados.
/// </summary>
[Trait("Category", "Unit")]
public sealed class DatabasePerformanceHealthCheckTests
{
    private readonly Mock<ILogger<DatabasePerformanceHealthCheck>> _loggerMock;

    public DatabasePerformanceHealthCheckTests()
    {
        _loggerMock = new Mock<ILogger<DatabasePerformanceHealthCheck>>();
    }

    [Fact]
    public async Task CheckHealthAsync_WithConfiguredMetrics_ShouldReturnHealthy()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database monitoring active");
        result.Data.Should().ContainKey("monitoring_active");
        result.Data.Should().ContainKey("check_window_minutes");
        result.Data.Should().ContainKey("metrics_configured");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIndicateMonitoringIsActive()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["monitoring_active"].Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeCheckWindowConfiguration()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["check_window_minutes"].Should().Be(5.0);
    }

    [Fact]
    public async Task CheckHealthAsync_WithConfiguredMetrics_ShouldIndicateMetricsConfigured()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["metrics_configured"].Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ShouldHandleGracefully()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result1 = await healthCheck.CheckHealthAsync(context);
        var result2 = await healthCheck.CheckHealthAsync(context);
        var result3 = await healthCheck.CheckHealthAsync(context);

        // Assert
        result1.Status.Should().Be(result2.Status).And.Be(result3.Status);
        result1.Description.Should().Be(result2.Description).And.Be(result3.Description);
        result1.Data["monitoring_active"].Should().Be(result2.Data["monitoring_active"])
            .And.Be(result3.Data["monitoring_active"]);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldCompleteQuickly()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();
        var startTime = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100),
            "health check should complete very quickly");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldHaveProperDataStructure()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().HaveCount(3, "should have monitoring_active, check_window_minutes, and metrics_configured");
        result.Data.Keys.Should().Contain("monitoring_active");
        result.Data.Keys.Should().Contain("check_window_minutes");
        result.Data.Keys.Should().Contain("metrics_configured");
    }

    [Fact]
    public async Task CheckHealthAsync_WithDifferentContexts_ShouldWorkCorrectly()
    {
        // Arrange
        using var meterFactory = new TestMeterFactory();
        var metrics = new DatabaseMetrics(meterFactory);
        var healthCheck = new DatabasePerformanceHealthCheck(metrics, _loggerMock.Object);
        var context1 = new HealthCheckContext();
        var context2 = new HealthCheckContext { Registration = new HealthCheckRegistration("test", healthCheck, null, null) };

        // Act
        var result1 = await healthCheck.CheckHealthAsync(context1);
        var result2 = await healthCheck.CheckHealthAsync(context2);

        // Assert
        result1.Status.Should().Be(HealthStatus.Healthy);
        result2.Status.Should().Be(HealthStatus.Healthy);
        result1.Description.Should().Be(result2.Description);
    }

    /// <summary>
    /// Simple test implementation of IMeterFactory for testing
    /// </summary>
    private class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options);
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
