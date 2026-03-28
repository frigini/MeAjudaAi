using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MeAjudaAi.Shared.Caching;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

public class FakeHybridCache : HybridCache
{
    public bool GetOrCreateAsyncCalled { get; set; }
    public bool SetAsyncCalled { get; set; }
    public bool RemoveAsyncCalled { get; set; }
    public bool RemoveByTagAsyncCalled { get; set; }
    public string? LastKey { get; set; }
    public object? LastValue { get; set; }

    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key, 
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory, 
        HybridCacheEntryOptions? options = null, 
        IEnumerable<string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        GetOrCreateAsyncCalled = true;
        LastKey = key;
        return factory(state, cancellationToken);
    }

    public override ValueTask SetAsync<T>(
        string key, 
        T value, 
        HybridCacheEntryOptions? options = null, 
        IEnumerable<string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        SetAsyncCalled = true;
        LastKey = key;
        LastValue = value;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        RemoveAsyncCalled = true;
        LastKey = key;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        RemoveByTagAsyncCalled = true;
        LastKey = tag;
        return ValueTask.CompletedTask;
    }
}

[Trait("Category", "Unit")]
public class HybridCacheServiceTests
{
    private readonly FakeHybridCache _hybridCache;
    private readonly Mock<ILogger<HybridCacheService>> _loggerMock;
    private readonly Mock<ICacheMetrics> _metricsMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly HybridCacheService _service;

    public HybridCacheServiceTests()
    {
        _hybridCache = new FakeHybridCache();
        _loggerMock = new Mock<ILogger<HybridCacheService>>();
        _metricsMock = new Mock<ICacheMetrics>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Default to cache enabled
        var cacheEnabledSection = new Mock<IConfigurationSection>();
        cacheEnabledSection.Setup(s => s.Value).Returns("true");
        _configurationMock.Setup(c => c.GetSection("Cache:Enabled")).Returns(cacheEnabledSection.Object);

        _service = new HybridCacheService(
            _hybridCache,
            _loggerMock.Object,
            _metricsMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenCacheDisabled_ShouldReturnDefaultAndNotCallCache()
    {
        // Arrange
        var cacheEnabledSection = new Mock<IConfigurationSection>();
        cacheEnabledSection.Setup(s => s.Value).Returns("false");
        _configurationMock.Setup(c => c.GetSection("Cache:Enabled")).Returns(cacheEnabledSection.Object);
        
        var service = new HybridCacheService(
            _hybridCache, _loggerMock.Object, _metricsMock.Object, _configurationMock.Object);

        // Act
        var (value, isCached) = await service.GetAsync<string>("test-key");

        // Assert
        isCached.Should().BeFalse();
        value.Should().BeNull();
        _hybridCache.GetOrCreateAsyncCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SetAsync_WithValidValue_ShouldCallHybridCache()
    {
        // Act
        await _service.SetAsync("key", "value", TimeSpan.FromMinutes(10));

        // Assert
        _hybridCache.SetAsyncCalled.Should().BeTrue();
        _hybridCache.LastKey.Should().Be("key");
        _hybridCache.LastValue.Should().Be("value");
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallHybridCache()
    {
        // Act
        await _service.RemoveAsync("key");

        // Assert
        _hybridCache.RemoveAsyncCalled.Should().BeTrue();
        _hybridCache.LastKey.Should().Be("key");
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCallFactoryOnCacheMiss()
    {
        // Arrange
        var factoryCalled = false;
        Func<CancellationToken, ValueTask<string>> factory = ct => { factoryCalled = true; return new ValueTask<string>("new-value"); };

        // Act
        var result = await _service.GetOrCreateAsync("key", factory);

        // Assert
        result.Should().Be("new-value");
        factoryCalled.Should().BeTrue();
        _hybridCache.GetOrCreateAsyncCalled.Should().BeTrue();
    }
}
