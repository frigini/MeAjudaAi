using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes E2E para o endpoint de busca de prestadores
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Search")]
public class SearchProvidersEndpointTests : TestContainerTestBase
{
    [Fact]
    public async Task SearchProviders_WithValidCoordinates_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 10;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound); // Aceitável se não há providers ainda

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            content.Should().NotBeNull();
            content!.Items.Should().NotBeNull();
            content.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            content.PageNumber.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchProviders_WithInvalidLatitude_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidLatitude = 91; // Latitude válida: -90 a 90
        var longitude = -46.6333;
        var radius = 10;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={invalidLatitude}&longitude={longitude}&radiusInKm={radius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithInvalidLongitude_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var invalidLongitude = 181; // Longitude válida: -180 a 180
        var radius = 10;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={invalidLongitude}&radiusInKm={radius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithNegativeRadius_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var invalidRadius = -5;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={invalidRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithRadiusExceeding500Km_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var excessiveRadius = 501;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={excessiveRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithMinRatingFilter_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;
        var minRating = 4.0;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&minRating={minRating}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            content.Should().NotBeNull();

            // Se houver resultados, todos devem ter rating >= minRating
            if (content!.Items.Any())
            {
                content.Items.Should().AllSatisfy(p =>
                    p.AverageRating.Should().BeGreaterThanOrEqualTo((decimal)minRating));
            }
        }
    }

    [Fact]
    public async Task SearchProviders_WithInvalidMinRating_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 10;
        var invalidRating = 5.5; // Rating válido: 0 a 5

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&minRating={invalidRating}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithServiceIdsFilter_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;
        var serviceId1 = Guid.NewGuid();
        var serviceId2 = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&serviceIds={serviceId1}&serviceIds={serviceId2}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchProviders_WithSubscriptionTiersFilter_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&subscriptionTiers=Premium&subscriptionTiers=Gold");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchProviders_WithPaginationPage1_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            content.Should().NotBeNull();
            content!.PageNumber.Should().Be(pageNumber);
            content.PageSize.Should().Be(pageSize);
            content.Items.Count.Should().BeLessThanOrEqualTo(pageSize);
        }
    }

    [Fact]
    public async Task SearchProviders_WithInvalidPageNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 10;
        var invalidPageNumber = 0; // PageNumber deve ser >= 1

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&pageNumber={invalidPageNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 10;
        var invalidPageSize = 101; // PageSize máximo: 100

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}&pageSize={invalidPageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProviders_WithAllFilters_ShouldReturnOk()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;
        var minRating = 4.0;
        var serviceId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 20;

        // Act
        var response = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}" +
            $"&minRating={minRating}&serviceIds={serviceId}&subscriptionTiers=Premium" +
            $"&pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            content.Should().NotBeNull();
            content!.PageNumber.Should().Be(pageNumber);
            content.PageSize.Should().Be(pageSize);
        }
    }

    [Fact]
    public async Task SearchProviders_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radius = 50;

        // Act - Fazer 3 chamadas idênticas
        var response1 = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}");
        var response2 = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}");
        var response3 = await ApiClient.GetAsync(
            $"/api/v1/search/providers?latitude={latitude}&longitude={longitude}&radiusInKm={radius}");

        // Assert - Todas devem ter o mesmo status code
        response1.StatusCode.Should().Be(response2.StatusCode);
        response2.StatusCode.Should().Be(response3.StatusCode);

        if (response1.StatusCode == HttpStatusCode.OK)
        {
            var content1 = await response1.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            var content2 = await response2.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);
            var content3 = await response3.Content.ReadFromJsonAsync<SearchProvidersResponse>(JsonOptions);

            // Total count deve ser consistente
            content1!.TotalCount.Should().Be(content2!.TotalCount);
            content2.TotalCount.Should().Be(content3!.TotalCount);
        }
    }

    [Fact]
    public async Task SearchProviders_WithMissingRequiredParameters_ShouldReturnBadRequest()
    {
        // Act - Chamada sem parâmetros obrigatórios
        var response = await ApiClient.GetAsync("/api/v1/search/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

/// <summary>
/// Response DTO para deserialização
/// </summary>
public class SearchProvidersResponse
{
    public List<SearchableProviderDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// DTO simplificado para deserialização
/// </summary>
public class SearchableProviderDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DistanceInKm { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string SubscriptionTier { get; set; } = string.Empty;
}
