using System.Diagnostics.Metrics;
using FluentAssertions;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

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
    public async Task CheckHealthAsync_WithCancelledToken_ShouldStillComplete()
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

        // Assert - Implementation completes synchronously and ignores cancellation
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        stopwatch.Stop();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1),
            "health check should complete quickly even under load");
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
        private readonly List<Meter> _meters = new();

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options);
            _meters.Add(meter);
            return meter;
        }

        public void Dispose()
        {
            foreach (var meter in _meters)
            {
                meter.Dispose();
            }
            _meters.Clear();
        }
    }
}
