using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Caching;

/// <summary>
/// Testes avançados para HybridCacheService cobrindo edge cases e cenários de erro
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Caching")]
public class HybridCacheServiceAdvancedTests
{
    private readonly Mock<HybridCache> _hybridCacheMock;
    private readonly Mock<ILogger<HybridCacheService>> _loggerMock;
    private readonly HybridCacheService _sut;

    public HybridCacheServiceAdvancedTests()
    {
        _hybridCacheMock = new Mock<HybridCache>();
        _loggerMock = new Mock<ILogger<HybridCacheService>>();
        _sut = new HybridCacheService(_hybridCacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCancellationToken_ShouldPassThroughCancellation()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _hybridCacheMock
            .Setup(x => x.GetOrCreateAsync<TestData>(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<TestData>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                token))
            .ReturnsAsync(value);

        // Act
        var result = await _sut.GetOrCreateAsync(
            key,
            _ => ValueTask.FromResult(value),
            cancellationToken: token);

        // Assert
        result.Should().Be(value);
        _hybridCacheMock.Verify(
            x => x.GetOrCreateAsync<TestData>(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<TestData>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                token),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _sut.GetOrCreateAsync<TestData>(
            null!,
            _ => ValueTask.FromResult(new TestData()));

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task GetOrCreateAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _sut.GetOrCreateAsync<TestData>(
            string.Empty,
            _ => ValueTask.FromResult(new TestData()));

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _sut.GetOrCreateAsync<TestData>(
            "test-key",
            null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenFactoryThrows_ShouldPropagateException()
    {
        // Arrange
        var key = "test-key";
        var exception = new InvalidOperationException("Factory error");

        _hybridCacheMock
            .Setup(x => x.GetOrCreateAsync<TestData>(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<TestData>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = () => _sut.GetOrCreateAsync(
            key,
            _ => throw exception);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Factory error");
    }

    [Fact]
    public async Task RemoveAsync_WithValidKey_ShouldCallHybridCache()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _sut.RemoveAsync(key);

        // Assert
        _hybridCacheMock.Verify(
            x => x.RemoveAsync(key, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _sut.RemoveAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task RemoveAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        var key = "test-key";
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _sut.RemoveAsync(key, token);

        // Assert
        _hybridCacheMock.Verify(
            x => x.RemoveAsync(key, token),
            Times.Once);
    }

    [Fact]
    public async Task RemoveByTagAsync_WithValidTags_ShouldCallHybridCache()
    {
        // Arrange
        var tags = new[] { "tag1", "tag2" };

        // Act
        await _sut.RemoveByTagAsync(tags);

        // Assert
        _hybridCacheMock.Verify(
            x => x.RemoveByTagAsync(
                It.Is<IEnumerable<string>>(t => t.SequenceEqual(tags)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveByTagAsync_WithNullTags_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _sut.RemoveByTagAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("tags");
    }

    [Fact]
    public async Task RemoveByTagAsync_WithEmptyTags_ShouldNotCallHybridCache()
    {
        // Arrange
        var tags = Array.Empty<string>();

        // Act
        await _sut.RemoveByTagAsync(tags);

        // Assert
        _hybridCacheMock.Verify(
            x => x.RemoveByTagAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCustomExpiration_ShouldUseProvidedOptions()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(30);

        TestData? capturedFactory = null;
        HybridCacheEntryOptions? capturedOptions = null;

        _hybridCacheMock
            .Setup(x => x.GetOrCreateAsync<TestData>(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<TestData>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<TestData>>, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                (k, factory, options, tags, ct) =>
                {
                    capturedOptions = options;
                    capturedFactory = factory(ct).AsTask().Result;
                })
            .ReturnsAsync(value);

        // Act
        var result = await _sut.GetOrCreateAsync(
            key,
            _ => ValueTask.FromResult(value),
            expiration);

        // Assert
        result.Should().Be(value);
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Expiration.Should().Be(expiration);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithTags_ShouldPassTagsToHybridCache()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };
        var tags = new[] { "tag1", "tag2" };

        IReadOnlyCollection<string>? capturedTags = null;

        _hybridCacheMock
            .Setup(x => x.GetOrCreateAsync<TestData>(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<TestData>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, ValueTask<TestData>>, HybridCacheEntryOptions, IReadOnlyCollection<string>, CancellationToken>(
                (k, factory, options, t, ct) => capturedTags = t)
            .ReturnsAsync(value);

        // Act
        var result = await _sut.GetOrCreateAsync(
            key,
            _ => ValueTask.FromResult(value),
            tags: tags);

        // Assert
        result.Should().Be(value);
        capturedTags.Should().NotBeNull();
        capturedTags.Should().BeEquivalentTo(tags);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
