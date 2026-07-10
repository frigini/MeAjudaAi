using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

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
    public async Task GetAsync_WhenCacheHit_ShouldReturnValue()
    {
        // Arrange
        _hybridCache.SimulateCacheHit = true;
        _hybridCache.HitValue = "cached-value";

        // Act
        var (value, isCached) = await _service.GetAsync<string>("test-key");

        // Assert
        isCached.Should().BeTrue();
        value.Should().Be("cached-value");
        _hybridCache.GetOrCreateAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WhenHybridCacheThrowsException_ShouldReturnDefault()
    {
        // Arrange
        _hybridCache.ExceptionToThrow = new Exception("Cache provider error");

        // Act
        var (value, isCached) = await _service.GetAsync<string>("test-key");

        // Assert
        isCached.Should().BeFalse();
        value.Should().BeNull();
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
    public async Task SetAsync_WithTags_ShouldCallHybridCacheWithTags()
    {
        // Arrange
        var tags = new[] { "tag1", "tag2" };

        // Act
        await _service.SetAsync("key", "value", tags: tags);

        // Assert
        _hybridCache.SetAsyncCalled.Should().BeTrue();
        _hybridCache.LastKey.Should().Be("key");
    }

    [Fact]
    public async Task SetAsync_WhenHybridCacheThrowsException_ShouldNotRethrow()
    {
        // Arrange
        _hybridCache.ExceptionToThrow = new Exception("Cache provider error");

        // Act & Assert - should not throw
        await _service.SetAsync("key", "value");
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
    public async Task RemoveAsync_WhenHybridCacheThrowsException_ShouldNotRethrow()
    {
        // Arrange
        _hybridCache.ExceptionToThrow = new Exception("Cache provider error");

        // Act & Assert - should not throw
        await _service.RemoveAsync("key");
    }

    [Fact]
    public async Task RemoveByTagAsync_ShouldCallHybridCache()
    {
        // Act
        await _service.RemoveByTagAsync("mytag");

        // Assert
        _hybridCache.RemoveByTagAsyncCalled.Should().BeTrue();
        _hybridCache.LastKey.Should().Be("mytag");
    }

    [Fact]
    public async Task RemoveByTagAsync_WhenHybridCacheThrowsException_ShouldNotRethrow()
    {
        // Arrange
        _hybridCache.ExceptionToThrow = new Exception("Cache provider error");

        // Act & Assert - should not throw
        await _service.RemoveByTagAsync("mytag");
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

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheDisabled_ShouldCallFactoryDirectly()
    {
        // Arrange
        var cacheEnabledSection = new Mock<IConfigurationSection>();
        cacheEnabledSection.Setup(s => s.Value).Returns("false");
        _configurationMock.Setup(c => c.GetSection("Cache:Enabled")).Returns(cacheEnabledSection.Object);

        var service = new HybridCacheService(
            _hybridCache, _loggerMock.Object, _metricsMock.Object, _configurationMock.Object);

        var factoryCalled = false;
        Func<CancellationToken, ValueTask<string>> factory = ct => { factoryCalled = true; return new ValueTask<string>("bypass-value"); };

        // Act
        var result = await service.GetOrCreateAsync("key", factory);

        // Assert
        result.Should().Be("bypass-value");
        factoryCalled.Should().BeTrue();
        _hybridCache.GetOrCreateAsyncCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenHybridCacheThrowsException_ShouldReturnDefault()
    {
        // Arrange
        _hybridCache.ExceptionToThrow = new Exception("Cache provider error");
        var factoryCalled = false;
        Func<CancellationToken, ValueTask<string>> factory = ct => { factoryCalled = true; return new ValueTask<string>("fallback-value"); };

        // Act
        var result = await _service.GetOrCreateAsync("key", factory);

        // Assert - when cache throws, it returns default without calling factory
        result.Should().BeNull();
        factoryCalled.Should().BeFalse();
    }
}
