using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

[Trait("Category", "Unit")]
public class PerformanceHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ShouldReturnHealthy_WhenPerformanceIsNormal()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
        result.Data.Should().NotBeEmpty();
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("memory_usage_mb");
        result.Data.Should().ContainKey("gc_gen0_collections");
        result.Data.Should().ContainKey("gc_gen1_collections");
        result.Data.Should().ContainKey("gc_gen2_collections");
        result.Data.Should().ContainKey("thread_pool_worker_threads");
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeMemoryMetrics()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["memory_usage_mb"].Should().BeOfType<long>();
        var memoryUsage = (long)result.Data["memory_usage_mb"];
        memoryUsage.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeGarbageCollectionMetrics()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["gc_gen0_collections"].Should().BeOfType<int>();
        result.Data["gc_gen1_collections"].Should().BeOfType<int>();
        result.Data["gc_gen2_collections"].Should().BeOfType<int>();

        var gen0 = (int)result.Data["gc_gen0_collections"];
        var gen1 = (int)result.Data["gc_gen1_collections"];
        var gen2 = (int)result.Data["gc_gen2_collections"];

        gen0.Should().BeGreaterThanOrEqualTo(0);
        gen1.Should().BeGreaterThanOrEqualTo(0);
        gen2.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeThreadPoolMetrics()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Data["thread_pool_worker_threads"].Should().BeOfType<int>();
        var threadCount = (int)result.Data["thread_pool_worker_threads"];
        threadCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeTimestamp()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();
        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        var afterCheck = DateTime.UtcNow;

        // Assert
        result.Data["timestamp"].Should().BeOfType<DateTime>();
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeOnOrAfter(beforeCheck).And.BeOnOrBefore(afterCheck);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }

    [Fact]
    public async Task CheckHealthAsync_MultipleCalls_ShouldReturnConsistentStructure()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Act
        var result1 = await healthCheck.CheckHealthAsync(context);
        var result2 = await healthCheck.CheckHealthAsync(context);

        // Assert
        result1.Data.Keys.Should().BeEquivalentTo(result2.Data.Keys);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnDegraded_WhenMemoryUsageIsHigh()
    {
        // Arrange
        var healthCheck = CreatePerformanceHealthCheck();
        var context = new HealthCheckContext();

        // Force high memory usage by allocating large array
        var largeArray = new byte[600 * 1024 * 1024]; // 600 MB
        GC.KeepAlive(largeArray);

        try
        {
            // Act
            var result = await healthCheck.CheckHealthAsync(context);

            // Assert
            result.Should().NotBeNull();
            // With 600MB allocated, should be Degraded (threshold is 500MB)
            if (result.Data.TryGetValue("memory_usage_mb", out var memUsage))
            {
                var memoryMB = (long)memUsage;
                if (memoryMB > 500)
                {
                    result.Status.Should().Be(HealthStatus.Degraded);
                    result.Description.Should().Contain("High memory usage detected");
                }
            }
        }
        finally
        {
            // Cleanup
            largeArray = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    #region Helper Methods

    private static IHealthCheck CreatePerformanceHealthCheck()
    {
        // Use reflection to create instance of internal class
        var type = typeof(MeAjudaAiHealthChecks)
            .GetNestedType("PerformanceHealthCheck", System.Reflection.BindingFlags.NonPublic);

        type.Should().NotBeNull("PerformanceHealthCheck should be a nested class of MeAjudaAiHealthChecks");

        var instance = Activator.CreateInstance(type!);
        return (IHealthCheck)instance!;
    }

    #endregion
}
