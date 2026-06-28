using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServiceByIdQueryTests
{
    [Fact]
    public void Constructor_WithValidId_ShouldCreateQuery()
    {
        var id = Guid.NewGuid();

        var query = new GetServiceByIdQuery(id);

        query.Should().NotBeNull();
        query.Id.Should().Be(id);
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        var id = Guid.NewGuid();
        var query = new GetServiceByIdQuery(id);

        var cacheKey = query.GetCacheKey();

        cacheKey.Should().Be($"service:{id}");
    }

    [Fact]
    public void GetCacheKey_WithDifferentIds_ShouldReturnDifferentKeys()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var query1 = new GetServiceByIdQuery(id1);
        var query2 = new GetServiceByIdQuery(id2);

        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        key1.Should().Be($"service:{id1}");
        key2.Should().Be($"service:{id2}");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn1Hour()
    {
        var query = new GetServiceByIdQuery(Guid.NewGuid());

        var expiration = query.GetCacheExpiration();

        expiration.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        var id = Guid.NewGuid();
        var query = new GetServiceByIdQuery(id);

        var tags = query.GetCacheTags();

        tags.Should().NotBeNull();
        tags.Should().Contain("service-catalogs");
        tags.Should().Contain($"service:{id}");
    }

    [Fact]
    public void Query_ShouldImplementICacheableQuery()
    {
        var query = new GetServiceByIdQuery(Guid.NewGuid());

        query.Should().BeAssignableTo<ICacheableQuery>();
    }

    [Fact]
    public void Query_ShouldBeQueryOfResult()
    {
        var query = new GetServiceByIdQuery(Guid.NewGuid());

        query.Should().BeAssignableTo<Query<Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceDto?>>>();
    }
}
