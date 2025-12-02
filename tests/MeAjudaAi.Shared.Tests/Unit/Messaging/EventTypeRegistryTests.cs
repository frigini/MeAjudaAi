using FluentAssertions;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.UnitTests.Messaging;

public class EventTypeRegistryTests
{
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<EventTypeRegistry>> _loggerMock;
    private readonly EventTypeRegistry _registry;

    public EventTypeRegistryTests()
    {
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<EventTypeRegistry>>();
        _registry = new EventTypeRegistry(_cacheMock.Object, _loggerMock.Object);
    }

    // Test event types
    public record TestIntegrationEvent(string Source) : IntegrationEvent(Source)
    {
        public string Data { get; init; } = string.Empty;
    }

    public record AnotherIntegrationEvent(string Source) : IntegrationEvent(Source)
    {
        public int Value { get; init; }
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldReturnDiscoveredEventTypes()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>
        {
            ["TestIntegrationEvent"] = typeof(TestIntegrationEvent),
            ["AnotherIntegrationEvent"] = typeof(AnotherIntegrationEvent)
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                "event-types-registry",
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetAllEventTypesAsync();

        // Assert
        result.Should().Contain(typeof(TestIntegrationEvent));
        result.Should().Contain(typeof(AnotherIntegrationEvent));
    }

    [Fact]
    public async Task GetEventTypeAsync_WithExistingEventName_ShouldReturnType()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>
        {
            ["TestIntegrationEvent"] = typeof(TestIntegrationEvent)
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetEventTypeAsync("TestIntegrationEvent");

        // Assert
        result.Should().Be(typeof(TestIntegrationEvent));
    }

    [Fact]
    public async Task GetEventTypeAsync_WithNonExistingEventName_ShouldReturnNull()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>
        {
            ["TestIntegrationEvent"] = typeof(TestIntegrationEvent)
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTypes);

        // Act
        var result = await _registry.GetEventTypeAsync("NonExistingEvent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateCacheAsync_ShouldRemoveCacheByPattern()
    {
        // Arrange
        _cacheMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _registry.InvalidateCacheAsync();

        // Assert
        _cacheMock.Verify(c => c.RemoveByPatternAsync("event-registry", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateCacheAsync_WithCancellationToken_ShouldPassTokenToCache()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken receivedToken = default;

        _cacheMock.Setup(c => c.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((_, ct) => receivedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        await _registry.InvalidateCacheAsync(cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldUseCacheWithOneHourExpiration()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>();
        TimeSpan? receivedExpiration = null;

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Dictionary<string, Type>>>, TimeSpan?, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                (_, _, exp, _, _, _) => receivedExpiration = exp)
            .ReturnsAsync(expectedTypes);

        // Act
        await _registry.GetAllEventTypesAsync();

        // Assert
        receivedExpiration.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldUseCorrectCacheKey()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>();
        string? receivedKey = null;

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Dictionary<string, Type>>>, TimeSpan?, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                (key, _, _, _, _, _) => receivedKey = key)
            .ReturnsAsync(expectedTypes);

        // Act
        await _registry.GetAllEventTypesAsync();

        // Assert
        receivedKey.Should().Be("event-types-registry");
    }

    [Fact]
    public async Task GetAllEventTypesAsync_ShouldTagCacheWithEventRegistry()
    {
        // Arrange
        var expectedTypes = new Dictionary<string, Type>();
        IReadOnlyCollection<string>? receivedTags = null;

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<Dictionary<string, Type>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<Dictionary<string, Type>>>, TimeSpan?, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                (_, _, _, _, tags, _) => receivedTags = tags)
            .ReturnsAsync(expectedTypes);

        // Act
        await _registry.GetAllEventTypesAsync();

        // Assert
        receivedTags.Should().Contain("event-registry");
    }
}
