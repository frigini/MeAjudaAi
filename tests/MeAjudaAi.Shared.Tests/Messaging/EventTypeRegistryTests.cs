using FluentAssertions;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Messaging;

public class EventTypeRegistryTests
{
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<EventTypeRegistry>> _mockLogger;
    private readonly EventTypeRegistry _registry;

    public EventTypeRegistryTests()
    {
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<EventTypeRegistry>>();
        _registry = new EventTypeRegistry(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldReturnCachedEventTypes()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>
        {
            ["TestEvent"] = typeof(TestIntegrationEvent)
        };

        _mockCache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetAllEventTypesAsync();

        // Assert
        result.Should().ContainSingle();
        result.Should().Contain(typeof(TestIntegrationEvent));
    }

    [Fact]
    public async Task GetEventTypeAsync_ShouldReturnEventType_WhenEventExists()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>
        {
            ["TestIntegrationEvent"] = typeof(TestIntegrationEvent)
        };

        _mockCache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetEventTypeAsync("TestIntegrationEvent");

        // Assert
        result.Should().Be(typeof(TestIntegrationEvent));
    }

    [Fact]
    public async Task GetEventTypeAsync_ShouldReturnNull_WhenEventDoesNotExist()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>();

        _mockCache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetEventTypeAsync("NonExistentEvent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldUseCacheWithCorrectKey()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>();

        _mockCache.Setup(x => x.GetOrCreateAsync(
                "event-types-registry",
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                TimeSpan.FromHours(1),
                It.Is<string[]>(tags => tags.Contains("event-registry")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        await _registry.GetAllEventTypesAsync();

        // Assert
        _mockCache.Verify(x => x.GetOrCreateAsync(
            "event-types-registry",
            It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
            TimeSpan.FromHours(1),
            It.Is<string[]>(tags => tags.Contains("event-registry")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateCacheAsync_ShouldCallCacheInvalidation()
    {
        // Arrange
        _mockCache.Setup(x => x.RemoveByTagAsync(
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _registry.InvalidateCacheAsync();

        // Assert
        _mockCache.Verify(x => x.RemoveByTagAsync(
            It.Is<string[]>(tags => tags.Contains("event-registry")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldDiscoverEventTypes_WhenCacheMisses()
    {
        // Arrange
        Dictionary<string, Type>? capturedTypes = null;

        _mockCache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<Dictionary<string, Type>>>, TimeSpan?, string[], CancellationToken>(
                async (key, factory, exp, tags, ct) =>
                {
                    capturedTypes = await factory();
                    return capturedTypes;
                });

        // Act
        var result = await _registry.GetAllEventTypesAsync();

        // Assert
        capturedTypes.Should().NotBeNull();
        capturedTypes!.Should().ContainKey("TestIntegrationEvent");
        result.Should().Contain(typeof(TestIntegrationEvent));
    }

    [Fact]
    public async Task GetEventTypeAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var expectedTypes = new Dictionary<string, Type>();

        _mockCache.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string[]>(),
                cts.Token))
            .ReturnsAsync(expectedTypes);

        // Act
        await _registry.GetEventTypeAsync("TestEvent", cts.Token);

        // Assert
        _mockCache.Verify(x => x.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<Dictionary<string, Type>>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<string[]>(),
            cts.Token), Times.Once);
    }

    // Test event for discovery
    private class TestIntegrationEvent : IntegrationEvent
    {
        public TestIntegrationEvent() : base(Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
}
