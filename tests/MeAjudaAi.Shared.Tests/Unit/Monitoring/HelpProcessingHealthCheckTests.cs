using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

/// <summary>
/// Testes para HelpProcessingHealthCheck - verifica se o sistema pode processar pedidos de ajuda.
/// Cobre componente de negócio, cenários de erro, validação de dados.
/// </summary>
[Trait("Category", "Unit")]
public sealed class HelpProcessingHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenSystemIsOperational_ShouldReturnHealthy()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Help processing system is operational");
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("component");
        result.Data.Should().ContainKey("can_process_requests");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeTimestamp()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["timestamp"].Should().NotBeNull();
        result.Data["timestamp"].Should().BeOfType<DateTime>();
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeComponentName()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["component"].Should().Be("help_processing");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIndicateProcessingCapability()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["can_process_requests"].Should().Be(true);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ShouldHandleGracefully()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert - even with cancellation, check should complete quickly
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result1 = await healthCheck.CheckHealthAsync(context);
        var result2 = await healthCheck.CheckHealthAsync(context);
        var result3 = await healthCheck.CheckHealthAsync(context);

        // Assert
        result1.Status.Should().Be(result2.Status).And.Be(result3.Status);
        result1.Description.Should().Be(result2.Description).And.Be(result3.Description);
        result1.Data["component"].Should().Be(result2.Data["component"]).And.Be(result3.Data["component"]);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldCompleteQuickly()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
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
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().HaveCount(3, "should have timestamp, component, and can_process_requests");
        result.Data.Keys.Should().Contain("timestamp");
        result.Data.Keys.Should().Contain("component");
        result.Data.Keys.Should().Contain("can_process_requests");
    }

    [Fact]
    public async Task CheckHealthAsync_WithDifferentContexts_ShouldWorkCorrectly()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
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
}
