using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Infrastructure;

[Trait("Category", "Integration")]
public class HealthChecksIntegrationTests
{
    #region PerformanceHealthCheck Tests

    [Fact]
    public async Task PerformanceHealthCheck_ShouldReturnHealthy()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }

    [Fact]
    public async Task PerformanceHealthCheck_ShouldIncludeMetricsInData()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("memory_usage_mb");
        result.Data.Should().ContainKey("gc_gen0_collections");
    }

    [Fact]
    public async Task PerformanceHealthCheck_MultipleChecks_ShouldComplete()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var results = new List<HealthCheckResult>();
        for (int i = 0; i < 3; i++)
        {
            results.Add(await healthCheck.CheckHealthAsync(context));
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Status != HealthStatus.Unhealthy);
    }

    #endregion

    #region HelpProcessingHealthCheck Tests

    [Fact]
    public async Task HelpProcessingHealthCheck_ShouldReturnHealthy()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task HelpProcessingHealthCheck_ShouldHaveDescription()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HelpProcessingHealthCheck_MultipleChecks_ShouldBeConsistent()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result1 = await healthCheck.CheckHealthAsync(context);
        var result2 = await healthCheck.CheckHealthAsync(context);

        // Assert
        result1.Status.Should().Be(result2.Status);
    }

    #endregion

    #region Health Check Context Tests

    [Fact]
    public async Task HealthCheck_WithCancelledToken_ShouldNotThrow()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        using var cts = new CancellationTokenSource();
        var context = new HealthCheckContext();

        cts.Cancel();

        // Act & Assert
        // Current implementation doesn't observe cancellation token, but should not throw
        var act = async () => await healthCheck.CheckHealthAsync(context, cts.Token);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HealthCheck_WithCustomRegistration_ShouldExecute()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var registration = new HealthCheckRegistration(
            "custom-check",
            _ => healthCheck,
            null,
            null);

        var context = new HealthCheckContext
        {
            Registration = registration
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().NotBe(HealthStatus.Unhealthy);
    }

    #endregion

    #region Concurrent Health Checks Tests

    [Fact]
    public async Task MultipleHealthChecks_RunConcurrently_ShouldSucceed()
    {
        // Arrange
        var perfCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        var helpCheck = new MeAjudaAiHealthChecks.HelpProcessingHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var tasks = new[]
        {
            perfCheck.CheckHealthAsync(context),
            helpCheck.CheckHealthAsync(context),
            perfCheck.CheckHealthAsync(context),
            helpCheck.CheckHealthAsync(context)
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(4);
        results.Should().OnlyContain(r => r.Status != HealthStatus.Unhealthy);
    }

    #endregion

    #region HangfireHealthCheck Tests

    [Fact]
    public async Task HangfireHealthCheck_ShouldReturnHealthy()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MeAjudaAiHealthChecks.HangfireHealthCheck>();
        var healthCheck = new MeAjudaAiHealthChecks.HangfireHealthCheck(logger);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("Hangfire is configured and operational");
    }

    [Fact]
    public async Task HangfireHealthCheck_ShouldIncludeMetadata()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MeAjudaAiHealthChecks.HangfireHealthCheck>();
        var healthCheck = new MeAjudaAiHealthChecks.HangfireHealthCheck(logger);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("component");
        result.Data.Should().ContainKey("configured");
        result.Data["component"].Should().Be("hangfire");
        result.Data["configured"].Should().Be(true);
    }

    [Fact]
    public async Task HangfireHealthCheck_MultipleChecks_ShouldBeConsistent()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MeAjudaAiHealthChecks.HangfireHealthCheck>();
        var healthCheck = new MeAjudaAiHealthChecks.HangfireHealthCheck(logger);
        var context = new HealthCheckContext();

        // Act - Execute multiple times
        var results = new List<HealthCheckResult>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(await healthCheck.CheckHealthAsync(context));
        }

        // Assert - All should be healthy and consistent
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => r.Status == HealthStatus.Healthy);
        results.Should().OnlyContain(r => r.Data.ContainsKey("configured"));
    }

    [Fact]
    public async Task HangfireHealthCheck_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new MeAjudaAiHealthChecks.HangfireHealthCheck(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Load and Stability Tests

    [Fact]
    public async Task HealthChecks_UnderLoad_ShouldRemainStable()
    {
        // Arrange
        var healthCheck = new MeAjudaAiHealthChecks.PerformanceHealthCheck();
        var context = new HealthCheckContext();
        var tasks = new List<Task<HealthCheckResult>>();

        // Act - Simulate load
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(healthCheck.CheckHealthAsync(context));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(20);

        // No results should be unhealthy (validates stability without being environment-dependent)
        results.Count(r => r.Status == HealthStatus.Unhealthy).Should().Be(0);
    }

    #endregion
}
