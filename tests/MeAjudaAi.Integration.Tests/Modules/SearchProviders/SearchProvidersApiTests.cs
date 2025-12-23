using System.Globalization;
using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.SearchProviders;

/// <summary>
/// Testes de integração para API de busca de provedores.
/// Valida mapeamento de endpoints, validação de parâmetros e tratamento de erros.
/// </summary>
/// <remarks>
/// Foca em validações de parâmetros e formato de requisição.
/// Não testa autenticação/autorização - endpoints de busca são públicos.
/// </remarks>
public class SearchProvidersApiTests : BaseApiTest
{
    private const string SearchEndpoint = "/api/v1/search/providers";

    [Fact]
    public async Task Search_WithValidCoordinates_ShouldNotReturnNotFound()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 5.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert - test that endpoint exists and accepts parameters
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "Search endpoint should be mapped");
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed,
            "GET method should be allowed");
    }

    [Fact]
    public async Task Search_WithoutRequiredParameters_ShouldReturnBadRequest()
    {
        // Act - missing required parameters
        var response = await Client.GetAsync(SearchEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Missing required parameters should return 400");
    }

    [Fact]
    public async Task Search_WithPagination_ShouldAcceptParameters()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 10.0;
        var page = 1;
        var pageSize = 5;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}&page={page}&pageSize={pageSize}");

        // Assert - test that pagination parameters are accepted
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Search_WithSmallRadius_ShouldAcceptParameter()
    {
        // Arrange - very small radius
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 0.5; // 500 meters

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "Small radius parameter should be accepted");
    }

    [Fact]
    public async Task Search_WithLargeRadius_ShouldAcceptParameter()
    {
        // Arrange - large radius
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 50.0; // 50km

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "Large radius parameter should be accepted");
    }

    [Fact]
    public async Task Search_WithMinRatingFilter_ShouldAcceptParameter()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 10.0;
        var minRating = 4.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}&minRating={minRating.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "minRating filter parameter should be accepted");
    }

    [Fact]
    public async Task Search_WithSubscriptionTierFilter_ShouldAcceptParameter()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 10.0;
        var subscriptionTier = 2; // Gold

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}&subscriptionTiers={subscriptionTier}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "subscriptionTiers filter parameter should be accepted");
    }

    [Fact]
    public async Task Search_WithMultipleFilters_ShouldAcceptParameters()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 10.0;
        var minRating = 3.5;
        var subscriptionTier = 1; // Standard

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}&minRating={minRating.ToString(CultureInfo.InvariantCulture)}&subscriptionTiers={subscriptionTier}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "Multiple filter parameters should be accepted");
    }

    [Fact]
    public async Task Search_WithDifferentLocation_ShouldAcceptCoordinates()
    {
        // Arrange - Rio de Janeiro coordinates
        var latitude = -22.9068;
        var longitude = -43.1729;
        var radiusInKm = 5.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "Different valid coordinates should be accepted");
    }

    [Fact]
    public async Task Search_WithInvalidLatitude_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidLatitude = 91; // Latitude válida: -90 a 90
        var longitude = -46.6333;
        var radiusInKm = 10.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={invalidLatitude}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid latitude (>90) should return 400 Bad Request");
    }

    [Fact]
    public async Task Search_WithInvalidLongitude_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var invalidLongitude = 181; // Longitude válida: -180 a 180
        var radiusInKm = 10.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={invalidLongitude}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid longitude (>180) should return 400 Bad Request");
    }

    [Fact]
    public async Task Search_WithNegativeRadius_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var invalidRadius = -5.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={invalidRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Negative radius should return 400 Bad Request");
    }

    [Fact]
    public async Task Search_WithRadiusExceeding500Km_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var excessiveRadius = 501.0;

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={excessiveRadius}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Radius exceeding 500km should return 400 Bad Request");
    }

    [Fact]
    public async Task Search_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var latitude = -23.5505;
        var longitude = -46.6333;
        var radiusInKm = 10.0;
        var invalidPageSize = 101; // Assuming max is 100

        // Act
        var response = await Client.GetAsync(
            $"{SearchEndpoint}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&radiusInKm={radiusInKm.ToString(CultureInfo.InvariantCulture)}&pageSize={invalidPageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Page size exceeding maximum should return 400 Bad Request");
    }
}
