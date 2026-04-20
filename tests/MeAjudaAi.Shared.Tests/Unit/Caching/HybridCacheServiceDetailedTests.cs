using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

[Trait("Category", "Unit")]
public class HybridCacheServiceDetailedTests
{
    private readonly Mock<HybridCache> _hybridCacheMock = new();
    private readonly Mock<ILogger<HybridCacheService>> _loggerMock = new();
    private readonly Mock<ICacheMetrics> _metricsMock = new();
    private readonly IConfiguration _enabledConfig;
    private readonly IConfiguration _disabledConfig;

    public HybridCacheServiceDetailedTests()
    {
        _enabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cache:Enabled"] = "true" })
            .Build();
            
        _disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cache:Enabled"] = "false" })
            .Build();
    }

    [Fact]
    public async Task GetAsync_WhenCacheDisabled_ShouldBypass()
    {
        // Arrange
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, _disabledConfig);

        // Act
        var (value, isCached) = await sut.GetAsync<string>("key");

        // Assert
        isCached.Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_WhenCacheThrowsException_ShouldReturnMiss()
    {
        // Arrange
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, _enabledConfig);
        _hybridCacheMock.Setup(c => c.GetOrCreateAsync<string>(
            It.IsAny<string>(), 
            It.IsAny<Func<CancellationToken, ValueTask<string>>>(), 
            It.IsAny<HybridCacheEntryOptions?>(), 
            It.IsAny<IEnumerable<string>?>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        var (value, isCached) = await sut.GetAsync<string>("key");

        // Assert
        isCached.Should().BeFalse();
        _metricsMock.Verify(m => m.RecordOperationDuration(It.IsAny<double>(), "get", "error"), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SetAsync_WhenCacheDisabled_ShouldBypass()
    {
        // Arrange
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, _disabledConfig);

        // Act
        await sut.SetAsync("key", "value");

        // Assert
        _hybridCacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HybridCacheEntryOptions?>(), It.IsAny<IEnumerable<string>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenExceptionOccurs_ShouldReturnDefault()
    {
        // Arrange
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, _enabledConfig);
        _hybridCacheMock.Setup(c => c.GetOrCreateAsync<string>(
            It.IsAny<string>(), 
            It.IsAny<Func<CancellationToken, ValueTask<string>>>(), 
            It.IsAny<HybridCacheEntryOptions?>(), 
            It.IsAny<IEnumerable<string>?>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache failure"));

        // Act
        var result = await sut.GetOrCreateAsync<string>("key", _ => new ValueTask<string>("fail"));

        // Assert
        result.Should().BeNull();
        _metricsMock.Verify(m => m.RecordOperationDuration(It.IsAny<double>(), "get-or-create", "error"), Times.AtLeastOnce);
    }
}
