using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServicesByCategoryQueryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQuery()
    {
        var categoryId = Guid.NewGuid();

        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: true);

        query.Should().NotBeNull();
        query.CategoryId.Should().Be(categoryId);
        query.ActiveOnly.Should().BeTrue();
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        var categoryId = Guid.NewGuid();
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: false);

        var cacheKey = query.GetCacheKey();

        cacheKey.Should().Be($"services:category:{categoryId}:active:False");
    }

    [Fact]
    public void GetCacheKey_WithDifferentParameters_ShouldReturnDifferentKeys()
    {
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var query1 = new GetServicesByCategoryQuery(categoryId1, ActiveOnly: true);
        var query2 = new GetServicesByCategoryQuery(categoryId2, ActiveOnly: false);

        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        key1.Should().Be($"services:category:{categoryId1}:active:True");
        key2.Should().Be($"services:category:{categoryId2}:active:False");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn1Hour()
    {
        var query = new GetServicesByCategoryQuery(Guid.NewGuid());

        var expiration = query.GetCacheExpiration();

        expiration.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        var categoryId = Guid.NewGuid();
        var query = new GetServicesByCategoryQuery(categoryId);

        var tags = query.GetCacheTags();

        tags.Should().NotBeNull();
        tags.Should().Contain("service-catalogs");
        tags.Should().Contain($"category:{categoryId}");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        var query = new GetServicesByCategoryQuery(Guid.NewGuid());

        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResult()
    {
        var query = new GetServicesByCategoryQuery(Guid.NewGuid());

        query.Should().BeAssignableTo<Query<Contracts.Functional.Result<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceListDto>>>>();
    }
}
