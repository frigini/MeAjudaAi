using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

[Trait("Category", "Unit")]
public class HybridCacheSimpleTests
{
    private readonly Mock<HybridCache> _hybridCacheMock = new();
    private readonly Mock<ILogger<HybridCacheService>> _loggerMock = new();
    private readonly Mock<ICacheMetrics> _metricsMock = new();

    [Fact]
    public async Task GetAsync_WhenCacheDisabled_ShouldReturnMiss()
    {
        // Arrange
        var disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cache:Enabled"] = "false" })
            .Build();
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, disabledConfig);

        // Act
        var (value, isCached) = await sut.GetAsync<string>("key");

        // Assert
        isCached.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCacheDisabled_ShouldCallFactoryDirectly()
    {
        // Arrange
        var disabledConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cache:Enabled"] = "false" })
            .Build();
        var sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object, _metricsMock.Object, disabledConfig);
        var factoryCalled = false;
        ValueTask<string> Factory(CancellationToken ct) { factoryCalled = true; return new ValueTask<string>("value"); }

        // Act
        var result = await sut.GetOrCreateAsync("key", Factory);

        // Assert
        result.Should().Be("value");
        factoryCalled.Should().BeTrue();
    }
}
