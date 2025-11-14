using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Queries;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para GetProvidersQuery (implementação de ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "Caching")]
public class GetProvidersQueryTests
{
    [Fact]
    public void GetCacheKey_WithBasicPagination_ShouldGenerateConsistentKey()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, null, null, null);

        // Act
        var cacheKey1 = query.GetCacheKey();
        var cacheKey2 = query.GetCacheKey();

        // Assert
        cacheKey1.Should().Be(cacheKey2);
        cacheKey1.Should().Contain("providers");
        cacheKey1.Should().Contain("page:1");
        cacheKey1.Should().Contain("size:10");
    }

    [Fact]
    public void GetCacheKey_WithName_ShouldIncludeNameInKey()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, "eletricista", null, null);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Contain("name:eletricista");
    }

    [Fact]
    public void GetCacheKey_WithType_ShouldIncludeTypeInKey()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, null, 1, null);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Contain("type:1");
    }

    [Fact]
    public void GetCacheKey_WithVerificationStatus_ShouldIncludeStatusInKey()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, null, null, 2);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Contain("status:2");
    }

    [Fact]
    public void GetCacheKey_WithAllFilters_ShouldIncludeAllInKey()
    {
        // Arrange
        var query = new GetProvidersQuery(2, 20, "encanador", 1, 2);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Contain("page:2");
        cacheKey.Should().Contain("size:20");
        cacheKey.Should().Contain("name:encanador");
        cacheKey.Should().Contain("type:1");
        cacheKey.Should().Contain("status:2");
    }

    [Fact]
    public void GetCacheKey_WithDifferentParameters_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var query1 = new GetProvidersQuery(1, 10, null, null, null);
        var query2 = new GetProvidersQuery(2, 10, null, null, null);

        // Act
        var cacheKey1 = query1.GetCacheKey();
        var cacheKey2 = query2.GetCacheKey();

        // Assert
        cacheKey1.Should().NotBe(cacheKey2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn5Minutes()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, null, null, null);

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnProvidersTag()
    {
        // Arrange
        var query = new GetProvidersQuery(1, 10, null, null, null);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().Contain("providers");
        tags.Should().Contain("providers-list");
    }

    [Fact]
    public void CorrelationId_ShouldBeUniqueForEachInstance()
    {
        // Arrange & Act
        var query1 = new GetProvidersQuery(1, 10, null, null, null);
        var query2 = new GetProvidersQuery(1, 10, null, null, null);

        // Assert
        query1.CorrelationId.Should().NotBe(query2.CorrelationId);
        query1.CorrelationId.Should().NotBeEmpty();
    }
}
