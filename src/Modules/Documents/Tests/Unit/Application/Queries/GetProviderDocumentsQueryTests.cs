using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para GetProviderDocumentsQuery (implementação de ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Component", "Caching")]
public class GetProviderDocumentsQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldGenerateConsistentKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var cacheKey1 = query.GetCacheKey();
        var cacheKey2 = query.GetCacheKey();

        // Assert
        cacheKey1.Should().Be(cacheKey2);
        cacheKey1.Should().Contain(providerId.ToString());
        cacheKey1.Should().Contain("provider");
        cacheKey1.Should().Contain("documents");
    }

    [Fact]
    public void GetCacheKey_WithDifferentProviders_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var providerId1 = Guid.NewGuid();
        var providerId2 = Guid.NewGuid();
        var query1 = new GetProviderDocumentsQuery(providerId1);
        var query2 = new GetProviderDocumentsQuery(providerId2);

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
        var query = new GetProviderDocumentsQuery(Guid.NewGuid());

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
        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().Contain("documents");
        tags.Should().Contain($"provider:{providerId}");
        tags.Should().Contain("provider-documents");
    }

    [Fact]
    public void CorrelationId_ShouldBeUniqueForEachInstance()
    {
        // Arrange & Act
        var query1 = new GetProviderDocumentsQuery(Guid.NewGuid());
        var query2 = new GetProviderDocumentsQuery(Guid.NewGuid());

        // Assert
        query1.CorrelationId.Should().NotBe(query2.CorrelationId);
        query1.CorrelationId.Should().NotBeEmpty();
        query2.CorrelationId.Should().NotBeEmpty();
    }
}
