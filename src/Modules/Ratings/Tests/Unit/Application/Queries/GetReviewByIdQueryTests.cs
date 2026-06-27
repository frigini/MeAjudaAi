using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class GetReviewByIdQueryTests
{
    [Fact]
    public void Constructor_WithValidIds_ShouldCreateQuery()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var query = new GetReviewByIdQuery(reviewId, correlationId);

        // Assert
        query.Should().NotBeNull();
        query.ReviewId.Should().Be(reviewId);
        query.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var query = new GetReviewByIdQuery(reviewId, Guid.NewGuid());

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"review:id:{reviewId}");
    }

    [Fact]
    public void GetCacheKey_WithDifferentReviewIds_ShouldReturnDifferentKeys()
    {
        // Arrange
        var query1 = new GetReviewByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var query2 = new GetReviewByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetReviewByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var query = new GetReviewByIdQuery(reviewId, Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("ratings");
        tags.Should().Contain($"review:{reviewId}");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetReviewByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultProviderReviewResponse()
    {
        // Arrange
        var query = new GetReviewByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<IQuery<Result<ProviderReviewResponse>>>();
    }
}
