using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Ratings")]
[Trait("Layer", "Application")]
public class GetProviderReviewsQueryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQuery()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;
        var correlationId = Guid.NewGuid();

        // Act
        var query = new GetProviderReviewsQuery(providerId, page, pageSize, correlationId);

        // Assert
        query.Should().NotBeNull();
        query.ProviderId.Should().Be(providerId);
        query.Page.Should().Be(page);
        query.PageSize.Should().Be(pageSize);
        query.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderReviewsQuery(providerId, 2, 5, Guid.NewGuid());

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"reviews:provider:{providerId}:page:2:size:5");
    }

    [Fact]
    public void GetCacheKey_WithDifferentParameters_ShouldReturnDifferentKeys()
    {
        // Arrange
        var query1 = new GetProviderReviewsQuery(Guid.NewGuid(), 1, 10, Guid.NewGuid());
        var query2 = new GetProviderReviewsQuery(Guid.NewGuid(), 1, 10, Guid.NewGuid());

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn5Minutes()
    {
        // Arrange
        var query = new GetProviderReviewsQuery(Guid.NewGuid(), 1, 10, Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderReviewsQuery(providerId, 1, 10, Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
        tags.Should().Contain("ratings");
        tags.Should().Contain($"provider-reviews:{providerId}");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        // Arrange
        var query = new GetProviderReviewsQuery(Guid.NewGuid(), 1, 10, Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResultPagedResultProviderReviewResponse()
    {
        // Arrange
        var query = new GetProviderReviewsQuery(Guid.NewGuid(), 1, 10, Guid.NewGuid());

        // Act & Assert
        query.Should().BeAssignableTo<IQuery<Result<PagedResult<ProviderReviewResponse>>>>();
    }
}
