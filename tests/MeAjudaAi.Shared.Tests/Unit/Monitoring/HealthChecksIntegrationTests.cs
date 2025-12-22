using FluentAssertions;
using MeAjudaAi.Shared.Jobs.HealthChecks;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

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

    #region HangfireHealthCheck Tests

    [Fact]
    public async Task HangfireHealthCheck_ShouldReturnDegradedWhenNotConfigured()
    {
        // Arrange - ServiceProvider sem Hangfire configurado
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var healthCheck = new HangfireHealthCheck(NullLogger<HangfireHealthCheck>.Instance, serviceProvider);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert - Deve retornar Degraded quando Hangfire não está configurado
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("not operational");
        result.Data.Should().ContainKey("error");
    }

    [Fact]
    public async Task HangfireHealthCheck_ShouldIncludeMetadata()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var healthCheck = new HangfireHealthCheck(NullLogger<HangfireHealthCheck>.Instance, serviceProvider);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert - Quando não configurado, deve incluir erro nos metadados
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("component");
        result.Data.Should().ContainKey("error");
        result.Data["component"].Should().Be("hangfire");
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task HangfireHealthCheck_MultipleChecks_ShouldBeConsistent()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var healthCheck = new HangfireHealthCheck(NullLogger<HangfireHealthCheck>.Instance, serviceProvider);
        var context = new HealthCheckContext();

        // Act - Execute multiple times
        var results = new List<HealthCheckResult>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(await healthCheck.CheckHealthAsync(context));
        }

        // Assert - Sem Hangfire configurado, todos devem retornar Degraded consistentemente
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => r.Status == HealthStatus.Degraded);
        results.Should().OnlyContain(r => r.Data.ContainsKey("error"));
        results.Should().OnlyContain(r => r.Description!.Contains("not operational"));
    }

    [Fact]
    public void HangfireHealthCheck_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var act = () => new HangfireHealthCheck(null!, serviceProvider);

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

    [Fact]
    public async Task HangfireHealthCheck_UnderLoad_ShouldRemainStable()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var healthCheck = new HangfireHealthCheck(NullLogger<HangfireHealthCheck>.Instance, serviceProvider);
        var context = new HealthCheckContext();
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => healthCheck.CheckHealthAsync(context));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert - Sem Hangfire configurado, todos devem retornar Degraded (não Unhealthy)
        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r.Status == HealthStatus.Degraded);
    }

    #endregion
}
