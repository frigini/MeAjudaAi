using MeAjudaAi.Shared.Behaviors;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Mediator;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Behaviors;

[Trait("Category", "Unit")]
public class CachingBehaviorTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>>>> _mockLogger;
    private readonly CachingBehavior<TestCacheableQuery, Result<string>> _behavior;

    public CachingBehaviorTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>>>>();
        _behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(_mockCacheService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotCacheable_ShouldBypassCacheAndExecuteNext()
    {
        // Arrange
        var nonCacheableQuery = new TestNonCacheableQuery();
        var behavior = new CachingBehavior<TestNonCacheableQuery, Result<string>>(_mockCacheService.Object, new Mock<ILogger<CachingBehavior<TestNonCacheableQuery, Result<string>>>>().Object);
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        var expectedResult = Result<string>.Success("test-result");
        next.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behavior.Handle(nonCacheableQuery, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        next.Verify(x => x(), Times.Once);
        _mockCacheService.Verify(x => x.GetAsync<Result<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResultAndNotExecuteNext()
    {
        // Arrange
        var query = new TestCacheableQuery("test-id");
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        var cachedResult = Result<string>.Success("cached-result");
        _mockCacheService.Setup(x => x.GetAsync<Result<string>>("test_cache_key", It.IsAny<CancellationToken>()))
                         .ReturnsAsync(cachedResult);

        // Act
        var result = await _behavior.Handle(query, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(cachedResult);
        next.Verify(x => x(), Times.Never);
        _mockCacheService.Verify(x => x.GetAsync<Result<string>>("test_cache_key", It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Result<string>>(), It.IsAny<TimeSpan?>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldExecuteNextAndCacheResult()
    {
        // Arrange
        var query = new TestCacheableQuery("test-id");
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        var queryResult = Result<string>.Success("query-result");

        _mockCacheService.Setup(x => x.GetAsync<Result<string>>("test_cache_key", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Result<string>?)null);
        next.Setup(x => x()).ReturnsAsync(queryResult);

        // Act
        var result = await _behavior.Handle(query, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(queryResult);
        next.Verify(x => x(), Times.Once);
        _mockCacheService.Verify(x => x.GetAsync<Result<string>>("test_cache_key", It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(
            "test_cache_key",
            queryResult,
            TimeSpan.FromMinutes(30),
            It.IsAny<HybridCacheEntryOptions>(),
            It.Is<IReadOnlyCollection<string>>(tags => tags.Contains("test-tag")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenQueryResultIsNull_ShouldNotCacheResult()
    {
        // Arrange
        var query = new TestCacheableQuery("test-id");
        var next = new Mock<RequestHandlerDelegate<Result<string>?>>();
        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>?>(_mockCacheService.Object, new Mock<ILogger<CachingBehavior<TestCacheableQuery, Result<string>?>>>().Object);

        _mockCacheService.Setup(x => x.GetAsync<Result<string>?>("test_cache_key", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Result<string>?)null);
        next.Setup(x => x()).ReturnsAsync((Result<string>?)null);

        // Act
        var result = await behavior.Handle(query, next.Object, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        next.Verify(x => x(), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Result<string>?>(), It.IsAny<TimeSpan?>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldConfigureHybridCacheOptionsCorrectly()
    {
        // Arrange
        var query = new TestCacheableQuery("test-id");
        var next = new Mock<RequestHandlerDelegate<Result<string>>>();
        var queryResult = Result<string>.Success("query-result");

        _mockCacheService.Setup(x => x.GetAsync<Result<string>>("test_cache_key", It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Result<string>?)null);
        next.Setup(x => x()).ReturnsAsync(queryResult);

        // Act
        await _behavior.Handle(query, next.Object, CancellationToken.None);

        // Assert
        _mockCacheService.Verify(x => x.SetAsync(
            "test_cache_key",
            queryResult,
            TimeSpan.FromMinutes(30),
            It.Is<HybridCacheEntryOptions>(opt => opt.LocalCacheExpiration == TimeSpan.FromMinutes(5)),
            It.Is<IReadOnlyCollection<string>>(tags => tags.Contains("test-tag")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test helper classes
    public class TestCacheableQuery(string id) : IRequest<Result<string>>, ICacheableQuery
    {
        public string Id { get; } = id;

        public string GetCacheKey() => "test_cache_key";
        public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(30);
        public IReadOnlyCollection<string> GetCacheTags() => ["test-tag"];
    }

    public class TestNonCacheableQuery : IRequest<Result<string>>
    {
        public string Id { get; set; } = "non-cacheable";
    }
}