using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

[Trait("Category", "Unit")]
public class EventTypeRegistryTests
{
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ILogger<EventTypeRegistry>> _loggerMock = new();
    private readonly EventTypeRegistry _sut;

    public EventTypeRegistryTests()
    {
        _sut = new EventTypeRegistry(_cacheMock.Object, _loggerMock.Object);
    }

    private record TestIntegrationEvent() : IntegrationEvent("Test");

    [Fact]
    public async Task GetEventTypeAsync_ShouldReturnNull_WhenTypeNotFound()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, string>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions?>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string>());

        // Act
        var result = await _sut.GetEventTypeAsync("NonExistentEvent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEventTypeAsync_ShouldReturnCorrectType_WhenTypeExistsInCache()
    {
        // Arrange
        var testType = typeof(TestIntegrationEvent);
        var cacheData = new Dictionary<string, string>
        {
            { testType.Name, testType.AssemblyQualifiedName! }
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, string>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions?>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cacheData);

        // Act
        var result = await _sut.GetEventTypeAsync(testType.Name);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(testType);
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldReturnTypes_WhenCacheHasData()
    {
        // Arrange
        var testType = typeof(TestIntegrationEvent);
        var cacheData = new Dictionary<string, string>
        {
            { testType.Name, testType.AssemblyQualifiedName! }
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, string>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<HybridCacheEntryOptions?>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cacheData);

        // Act
        var result = await _sut.GetAllEventTypesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(testType);
    }

    [Fact]
    public async Task InvalidateCacheAsync_ShouldCallCacheRemove()
    {
        // Act
        await _sut.InvalidateCacheAsync();

        // Assert
        _cacheMock.Verify(c => c.RemoveByPatternAsync("event-registry", It.IsAny<CancellationToken>()), Times.Once);
    }
}
