using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

[Trait("Category", "Unit")]
public class CacheMetricsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMeterFactory _meterFactory;
    private readonly CacheMetrics _metrics;

    public CacheMetricsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMetrics();
        
        _serviceProvider = services.BuildServiceProvider();
        _meterFactory = _serviceProvider.GetRequiredService<IMeterFactory>();
        
        _metrics = new CacheMetrics(_meterFactory);
    }

    [Fact]
    public void Constructor_ShouldInitializeMetricsCorrectly()
    {
        // Act & Assert - O construtor não deve lançar exceção
        var metrics = new CacheMetrics(_meterFactory);
        metrics.Should().NotBeNull();
    }

    [Fact]
    public void RecordCacheHit_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";
        var operation = "get";

        // Act & Assert
        var action = () => _metrics.RecordCacheHit(key, operation);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordCacheHit_WithDefaultOperation_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";

        // Act & Assert
        var action = () => _metrics.RecordCacheHit(key);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordCacheMiss_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";
        var operation = "get";

        // Act & Assert
        var action = () => _metrics.RecordCacheMiss(key, operation);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordCacheMiss_WithDefaultOperation_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";

        // Act & Assert
        var action = () => _metrics.RecordCacheMiss(key);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordOperationDuration_ShouldNotThrow()
    {
        // Arrange
        var duration = 0.125; // 125ms
        var operation = "set";
        var result = "success";

        // Act & Assert
        var action = () => _metrics.RecordOperationDuration(duration, operation, result);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordOperation_WithCacheHit_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";
        var operation = "get-or-create";
        var isHit = true;
        var duration = 0.025; // 25ms

        // Act & Assert
        var action = () => _metrics.RecordOperation(key, operation, isHit, duration);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordOperation_WithCacheMiss_ShouldNotThrow()
    {
        // Arrange
        var key = "test-key";
        var operation = "get-or-create";
        var isHit = false;
        var duration = 0.250; // 250ms

        // Act & Assert
        var action = () => _metrics.RecordOperation(key, operation, isHit, duration);
        action.Should().NotThrow();
    }

    [Fact]
    public void RecordMultipleOperations_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        var action = () =>
        {
            _metrics.RecordCacheHit("key1", "get");
            _metrics.RecordCacheMiss("key2", "get");
            _metrics.RecordOperationDuration(0.1, "get", "hit");
            _metrics.RecordOperationDuration(0.2, "get", "miss");
            _metrics.RecordOperation("key3", "set", true, 0.05);
            _metrics.RecordOperation("key4", "set", false, 0.15);
        };
        
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("cache-key-1", "get")]
    [InlineData("cache-key-2", "set")]
    [InlineData("cache-key-3", "delete")]
    [InlineData("very-long-cache-key-with-special-characters-!@#$%", "get-or-create")]
    public void RecordCacheHit_WithVariousKeys_ShouldNotThrow(string key, string operation)
    {
        // Act & Assert
        var action = () => _metrics.RecordCacheHit(key, operation);
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(0.001, "get", "hit")]
    [InlineData(0.1, "set", "success")]
    [InlineData(1.0, "get", "miss")]
    [InlineData(5.5, "delete", "error")]
    public void RecordOperationDuration_WithVariousDurations_ShouldNotThrow(double duration, string operation, string result)
    {
        // Act & Assert
        var action = () => _metrics.RecordOperationDuration(duration, operation, result);
        action.Should().NotThrow();
    }

    [Fact]
    public void CacheMetrics_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var tasks = new List<Task>();
        var random = new Random();

        // Act - Cria m�ltiplas opera��es concorrentes
        for (int i = 0; i < 100; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                var key = $"concurrent-key-{taskId}";
                var isHit = random.Next(0, 2) == 1;
                var duration = random.NextDouble() * 0.5; // 0-500ms
                
                _metrics.RecordOperation(key, "concurrent-test", isHit, duration);
            }));
        }

        // Assert
        var action = () => Task.WaitAll(tasks.ToArray());
        action.Should().NotThrow();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
