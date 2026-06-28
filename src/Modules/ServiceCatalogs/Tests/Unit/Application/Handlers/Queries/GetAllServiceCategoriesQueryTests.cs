using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetAllServiceCategoriesQueryTests
{
    [Fact]
    public void Constructor_WithValidActiveOnly_ShouldCreateQuery()
    {
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: true);

        query.Should().NotBeNull();
        query.ActiveOnly.Should().BeTrue();
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);

        var cacheKey = query.GetCacheKey();

        cacheKey.Should().Be("categories:all:active:False");
    }

    [Fact]
    public void GetCacheKey_WithDifferentActiveOnly_ShouldReturnDifferentKeys()
    {
        var query1 = new GetAllServiceCategoriesQuery(ActiveOnly: true);
        var query2 = new GetAllServiceCategoriesQuery(ActiveOnly: false);

        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        key1.Should().Be("categories:all:active:True");
        key2.Should().Be("categories:all:active:False");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn2Hours()
    {
        var query = new GetAllServiceCategoriesQuery();

        var expiration = query.GetCacheExpiration();

        expiration.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnServiceCatalogsTag()
    {
        var query = new GetAllServiceCategoriesQuery();

        var tags = query.GetCacheTags();

        tags.Should().NotBeNull();
        tags.Should().Contain("service-catalogs");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        var query = new GetAllServiceCategoriesQuery();

        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResult()
    {
        var query = new GetAllServiceCategoriesQuery();

        query.Should().BeAssignableTo<Query<Contracts.Functional.Result<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto>>>>();
    }
}
