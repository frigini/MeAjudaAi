using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetAllServicesQueryTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateQuery()
    {
        var query = new GetAllServicesQuery(ActiveOnly: true, Name: "Limpeza");

        query.Should().NotBeNull();
        query.ActiveOnly.Should().BeTrue();
        query.Name.Should().Be("Limpeza");
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        var query = new GetAllServicesQuery(ActiveOnly: false, Name: null);

        var cacheKey = query.GetCacheKey();

        cacheKey.Should().Be("services:all:active:False:name:");
    }

    [Fact]
    public void GetCacheKey_WithDifferentParameters_ShouldReturnDifferentKeys()
    {
        var query1 = new GetAllServicesQuery(ActiveOnly: true, Name: "Limpeza");
        var query2 = new GetAllServicesQuery(ActiveOnly: false, Name: "Reparos");

        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        key1.Should().Be("services:all:active:True:name:Limpeza");
        key2.Should().Be("services:all:active:False:name:Reparos");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn2Hours()
    {
        var query = new GetAllServicesQuery();

        var expiration = query.GetCacheExpiration();

        expiration.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnServiceCatalogsTag()
    {
        var query = new GetAllServicesQuery();

        var tags = query.GetCacheTags();

        tags.Should().NotBeNull();
        tags.Should().Contain("service-catalogs");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        var query = new GetAllServicesQuery();

        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResult()
    {
        var query = new GetAllServicesQuery();

        query.Should().BeAssignableTo<Query<Contracts.Functional.Result<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceListDto>>>>();
    }
}
