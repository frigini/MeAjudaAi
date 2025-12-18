using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Queries;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para GetProvidersByIdsQuery (implementação de ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "Caching")]
public class GetProvidersByIdsQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldGenerateConsistentKey()
    {
        // Arrange
        var providerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var cacheKey1 = query.GetCacheKey();
        var cacheKey2 = query.GetCacheKey();

        // Assert
        cacheKey1.Should().Be(cacheKey2);
        cacheKey1.Should().Contain("providers");
        cacheKey1.Should().Contain("ids");
    }

    [Fact]
    public void GetCacheKey_WithDifferentIds_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var providerIds1 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var providerIds2 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query1 = new GetProvidersByIdsQuery(providerIds1);
        var query2 = new GetProvidersByIdsQuery(providerIds2);

        // Act
        var cacheKey1 = query1.GetCacheKey();
        var cacheKey2 = query2.GetCacheKey();

        // Assert
        cacheKey1.Should().NotBe(cacheKey2);
    }

    [Fact]
    public void GetCacheKey_WithSameIdsInDifferentOrder_ShouldGenerateSameKey()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var query1 = new GetProvidersByIdsQuery(new List<Guid> { id1, id2 });
        var query2 = new GetProvidersByIdsQuery(new List<Guid> { id2, id1 });

        // Act
        var cacheKey1 = query1.GetCacheKey();
        var cacheKey2 = query2.GetCacheKey();

        // Assert - Assuming the implementation sorts IDs for consistent caching
        cacheKey1.Should().Be(cacheKey2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var providerIds = new List<Guid> { Guid.NewGuid() };
        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheKey_WithEmptyList_ShouldGenerateKey()
    {
        // Arrange
        var query = new GetProvidersByIdsQuery(new List<Guid>());

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().NotBeNullOrEmpty();
        cacheKey.Should().Contain("providers");
    }

    [Fact]
    public void GetCacheKey_WithSingleId_ShouldIncludeIdInKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProvidersByIdsQuery(new List<Guid> { providerId });

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Contain(providerId.ToString());
    }
}
