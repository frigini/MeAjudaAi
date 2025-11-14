using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Queries;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para GetProviderByIdQuery (implementação de ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "Caching")]
public class GetProviderByIdQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldGenerateConsistentKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderByIdQuery(providerId);

        // Act
        var cacheKey1 = query.GetCacheKey();
        var cacheKey2 = query.GetCacheKey();

        // Assert
        cacheKey1.Should().Be(cacheKey2);
        cacheKey1.Should().Contain(providerId.ToString());
        cacheKey1.Should().Contain("provider");
    }

    [Fact]
    public void GetCacheKey_WithDifferentIds_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var providerId1 = Guid.NewGuid();
        var providerId2 = Guid.NewGuid();
        var query1 = new GetProviderByIdQuery(providerId1);
        var query2 = new GetProviderByIdQuery(providerId2);

        // Act
        var cacheKey1 = query1.GetCacheKey();
        var cacheKey2 = query2.GetCacheKey();

        // Assert
        cacheKey1.Should().NotBe(cacheKey2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetProviderByIdQuery(Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnProvidersTags()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderByIdQuery(providerId);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().Contain("providers");
        tags.Should().Contain($"provider:{providerId}");
    }

    [Fact]
    public void CorrelationId_ShouldBeUniqueForEachInstance()
    {
        // Arrange & Act
        var query1 = new GetProviderByIdQuery(Guid.NewGuid());
        var query2 = new GetProviderByIdQuery(Guid.NewGuid());

        // Assert
        query1.CorrelationId.Should().NotBe(query2.CorrelationId);
        query1.CorrelationId.Should().NotBeEmpty();
    }
}
