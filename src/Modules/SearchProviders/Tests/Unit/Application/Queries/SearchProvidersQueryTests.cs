using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Application.Queries;

/// <summary>
/// Testes unitários para SearchProvidersQuery (ICacheableQuery)
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Search")]
[Trait("Component", "Query")]
public class SearchProvidersQueryTests
{
    [Fact]
    public void GetCacheKey_WithBasicParameters_ShouldGenerateConsistentKey()
    {
        // Arrange
        var query = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10);

        // Act
        var key1 = query.GetCacheKey();
        var key2 = query.GetCacheKey();

        // Assert
        key1.Should().Be(key2);
        key1.Should().Contain("search:providers");
        key1.Should().Contain("lat:-23.5505");
        key1.Should().Contain("lng:-46.6333");
        key1.Should().Contain("radius:10");
    }

    [Fact]
    public void GetCacheKey_WithSameServicesInDifferentOrder_ShouldGenerateSameKey()
    {
        // Arrange
        var serviceId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var serviceId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var query1 = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            ServiceIds: new[] { serviceId1, serviceId2 });

        var query2 = new SearchProvidersQuery(
            Latitude: -23.5505,
            Longitude: -46.6333,
            RadiusInKm: 10,
            ServiceIds: new[] { serviceId2, serviceId1 });

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GetCacheKey_WithDifferentCoordinates_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var query1 = new SearchProvidersQuery(-23.5505, -46.6333, 10);
        var query2 = new SearchProvidersQuery(-22.9068, -43.1729, 10);

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void GetCacheKey_RoundsCoordinatesTo4DecimalPlaces()
    {
        // Arrange
        var query1 = new SearchProvidersQuery(-23.55051234, -46.63331234, 10);
        var query2 = new SearchProvidersQuery(-23.55052345, -46.63332345, 10);

        // Act
        var key1 = query1.GetCacheKey();
        var key2 = query2.GetCacheKey();

        // Assert - Deve arredondar para as mesmas 4 casas decimais
        key1.Should().Be(key2);
        key1.Should().Contain("lat:-23.5505");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn5Minutes()
    {
        // Arrange
        var query = new SearchProvidersQuery(-23.5505, -46.6333, 10);

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnSearchProvidersAndSearchResultsTags()
    {
        // Arrange
        var query = new SearchProvidersQuery(-23.5505, -46.6333, 10);

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().Contain("search");
        tags.Should().Contain("providers");
        tags.Should().Contain("search-results");
    }

    [Fact]
    public void CorrelationId_ShouldBeUniqueForEachInstance()
    {
        // Arrange & Act
        var query1 = new SearchProvidersQuery(-23.5505, -46.6333, 10);
        var query2 = new SearchProvidersQuery(-23.5505, -46.6333, 10);

        // Assert
        query1.CorrelationId.Should().NotBe(query2.CorrelationId);
        query1.CorrelationId.Should().NotBeEmpty();
    }
}
