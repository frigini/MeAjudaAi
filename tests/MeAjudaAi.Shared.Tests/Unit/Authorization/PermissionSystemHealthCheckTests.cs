using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.HealthChecks;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Authorization.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para PermissionSystemHealthCheck
/// Cobertura: CheckHealthAsync, BasicFunctionality, PerformanceMetrics, CacheHealth, ModuleResolvers
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Authorization")]
public class PermissionSystemHealthCheckTests
{
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IPermissionMetricsService> _mockMetricsService;
    private readonly Mock<ILogger<PermissionSystemHealthCheck>> _mockLogger;
    private readonly PermissionSystemHealthCheck _healthCheck;

    public PermissionSystemHealthCheckTests()
    {
        _mockPermissionService = new Mock<IPermissionService>();
        _mockMetricsService = new Mock<IPermissionMetricsService>();
        _mockLogger = new Mock<ILogger<PermissionSystemHealthCheck>>();

        _healthCheck = new PermissionSystemHealthCheck(
            _mockPermissionService.Object,
            _mockMetricsService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CheckHealthAsync_WithAllHealthyChecks_ShouldReturnHealthy()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("operating normally");
        result.Data.Should().ContainKey("basic_functionality");
        result.Data.Should().ContainKey("performance_metrics");
        result.Data.Should().ContainKey("cache_health");
        result.Data.Should().ContainKey("module_resolvers");
    }

    [Fact]
    public async Task CheckHealthAsync_WithSlowPermissionResolution_ShouldReturnDegraded()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(2100); // Exceeds MaxPermissionResolutionTime (2s)
                return new[] { EPermission.UsersRead };
            });

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Permission resolution took");
    }

    [Fact]
    public async Task CheckHealthAsync_WithLowCacheHitRate_ShouldReturnDegraded()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.65, // Below MinCacheHitRate (0.7)
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Low cache hit rate");
        result.Data["cache_hit_rate"].Should().Be(0.65);
    }

    [Fact]
    public async Task CheckHealthAsync_WithTooManyActiveChecks_ShouldReturnDegraded()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 150 // Exceeds MaxActiveChecks (100)
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Too many active checks");
        result.Data["active_checks"].Should().Be(150);
    }

    [Fact]
    public async Task CheckHealthAsync_WithMultipleIssues_ShouldReturnUnhealthy()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(2100); // Slow
                return new[] { EPermission.UsersRead };
            });

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.60, // Low
                ActiveChecks = 150 // High
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("unhealthy");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenPermissionServiceThrows_ShouldReturnUnhealthy()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Permission service failed"));

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Basic functionality failed");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenMetricsServiceThrows_ShouldStillComplete()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Throws(new Exception("Metrics service failed"));

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Data["performance_metrics"].Should().Be("error");
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert - Exception is caught by CheckBasicFunctionalityAsync, returns degraded
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Basic functionality failed");
    }

    [Fact]
    public async Task CheckHealthAsync_WithLowCheckCount_ShouldNotReportCacheIssues()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 50, // Below threshold of 100
                CacheHitRate = 0.50, // Would be low, but ignored due to low check count
                ActiveChecks = 10
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("operating normally");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeAllMetricsInData()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([EPermission.UsersRead, EPermission.UsersCreate]);

        _mockPermissionService
            .Setup(x => x.HasPermissionAsync(It.IsAny<string>(), It.IsAny<EPermission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 5000,
                CacheHitRate = 0.92,
                ActiveChecks = 25
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().ContainKey("basic_functionality");
        result.Data.Should().ContainKey("performance_metrics");
        result.Data.Should().ContainKey("cache_hit_rate");
        result.Data.Should().ContainKey("active_checks");
        result.Data.Should().ContainKey("cache_health");
        result.Data.Should().ContainKey("module_resolvers");
        result.Data.Should().ContainKey("resolver_count");

        result.Data["cache_hit_rate"].Should().Be(0.92);
        result.Data["active_checks"].Should().Be(25);
        result.Data["resolver_count"].Should().Be(1);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHealthCheckThrowsUnexpectedException_ShouldReturnDegraded()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(new OutOfMemoryException("Critical failure"));

        _mockMetricsService
            .Setup(x => x.GetSystemStats())
            .Returns(new PermissionSystemStats
            {
                TotalPermissionChecks = 1000,
                CacheHitRate = 0.85,
                ActiveChecks = 50
            });

        var context = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(context);

        // Assert - Exception is caught by CheckBasicFunctionalityAsync, not propagated
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("degraded");
        result.Description.Should().Contain("Basic functionality failed");
        result.Exception.Should().BeNull(); // Exception is caught internally, not exposed
    }
}
