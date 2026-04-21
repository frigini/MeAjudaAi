using MeAjudaAi.Shared.Authorization.HealthChecks;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Authorization.HealthChecks.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.HealthChecks;

[Trait("Category", "Unit")]
public class PermissionSystemHealthCheckTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock = new();
    private readonly Mock<IPermissionMetricsService> _metricsServiceMock = new();
    private readonly Mock<ILogger<PermissionSystemHealthCheck>> _loggerMock = new();
    private readonly PermissionSystemHealthCheck _sut;

    public PermissionSystemHealthCheckTests()
    {
        _sut = new PermissionSystemHealthCheck(_permissionServiceMock.Object, _metricsServiceMock.Object, _loggerMock.Object);
        
        // Default stats setup
        _metricsServiceMock.Setup(m => m.GetSystemStats()).Returns(new PermissionSystemStats());
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDependenciesOk_ShouldReturnHealthy()
    {
        // Arrange
        var context = new HealthCheckContext();

        // Act
        var result = await _sut.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("basic_functionality");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenServiceFails_ShouldReturnDegraded()
    {
        // Arrange
        _permissionServiceMock.Setup(p => p.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service down"));
        var context = new HealthCheckContext();

        // Act
        var result = await _sut.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
    }
}
