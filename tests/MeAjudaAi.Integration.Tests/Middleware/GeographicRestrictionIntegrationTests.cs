using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using System.Net;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Middleware;

[Collection("Integration")]
public class GeographicRestrictionIntegrationTests : ApiTestBase
{
    [Fact(Skip = "Requires IGeographicValidationService (IBGE) mock setup in WebApplicationFactory - TODO: Configure in ApiTestBase")]
    public async Task GetProviders_WhenAllowedCity_ShouldReturnOk()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "Muriaé");
        Client.DefaultRequestHeaders.Add("X-User-State", "MG");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact(Skip = "Requires IGeographicValidationService (IBGE) mock setup in WebApplicationFactory - TODO: Configure in ApiTestBase")]
    public async Task GetProviders_WhenBlockedCity_ShouldReturn451()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "São Paulo");
        Client.DefaultRequestHeaders.Add("X-User-State", "SP");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons); // 451

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        json.GetProperty("error").GetString().Should().Be("geographic_restriction");
        json.GetProperty("message").GetString().Should().Contain("Muriaé");
        json.GetProperty("allowedCities").GetArrayLength().Should().Be(3);
        json.GetProperty("yourLocation").GetProperty("city").GetString().Should().Be("São Paulo");
    }

    [Fact(Skip = "Requires IGeographicValidationService (IBGE) mock setup in WebApplicationFactory - TODO: Configure in ApiTestBase")]
    public async Task GetProviders_WhenAllowedState_ShouldReturnOk()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-State", "MG");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact(Skip = "Requires IGeographicValidationService (IBGE) mock setup in WebApplicationFactory - TODO: Configure in ApiTestBase")]
    public async Task GetProviders_WhenLocationHeader_ShouldReturnOk()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-Location", "Itaperuna|RJ");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task HealthCheck_ShouldAlwaysWork_RegardlessOfLocation()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "Invalid City");
        Client.DefaultRequestHeaders.Add("X-User-State", "XX");

        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swagger_ShouldAlwaysWork_RegardlessOfLocation()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "Invalid City");

        // Act
        var response = await Client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound); // NotFound if swagger disabled
    }

    [Theory]
    [InlineData("Muriaé", "MG")]
    [InlineData("muriaé", "mg")] // Case insensitive
    [InlineData("MURIAÉ", "MG")]
    [InlineData("Itaperuna", "RJ")]
    [InlineData("itaperuna", "rj")]
    [InlineData("Linhares", "ES")]
    public async Task GetProviders_WithAllowedCities_CaseInsensitive_ShouldWork(string city, string state)
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", city);
        Client.DefaultRequestHeaders.Add("X-User-State", state);

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
            $"City '{city}' in state '{state}' should be allowed");
    }
}
