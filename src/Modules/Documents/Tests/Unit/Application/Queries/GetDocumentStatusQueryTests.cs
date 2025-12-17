using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para GetDocumentStatusQuery (implementação de ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Component", "Caching")]
public class GetDocumentStatusQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldGenerateConsistentKey()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var query = new GetDocumentStatusQuery(documentId);

        // Ação
        var cacheKey1 = query.GetCacheKey();
        var cacheKey2 = query.GetCacheKey();

        // Verificação
        cacheKey1.Should().Be(cacheKey2);
        cacheKey1.Should().Contain(documentId.ToString());
        cacheKey1.Should().Contain("document");
        cacheKey1.Should().Contain("status");
    }

    [Fact]
    public void GetCacheKey_WithDifferentIds_ShouldGenerateDifferentKeys()
    {
        // Preparação
        var documentId1 = Guid.NewGuid();
        var documentId2 = Guid.NewGuid();
        var query1 = new GetDocumentStatusQuery(documentId1);
        var query2 = new GetDocumentStatusQuery(documentId2);

        // Ação
        var cacheKey1 = query1.GetCacheKey();
        var cacheKey2 = query2.GetCacheKey();

        // Verificação
        cacheKey1.Should().NotBe(cacheKey2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn2Minutes()
    {
        // Preparação
        var query = new GetDocumentStatusQuery(Guid.NewGuid());

        // Ação
        var expiration = query.GetCacheExpiration();

        // Verificação
        expiration.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnDocumentsTags()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var query = new GetDocumentStatusQuery(documentId);

        // Ação
        var tags = query.GetCacheTags();

        // Verificação
        tags.Should().Contain("documents");
        tags.Should().Contain($"document:{documentId}");
    }

    [Fact]
    public void CorrelationId_ShouldBeUniqueForEachInstance()
    {
        // Preparação & Act
        var query1 = new GetDocumentStatusQuery(Guid.NewGuid());
        var query2 = new GetDocumentStatusQuery(Guid.NewGuid());

        // Verificação
        query1.CorrelationId.Should().NotBe(query2.CorrelationId);
        query1.CorrelationId.Should().NotBeEmpty();
    }
}
