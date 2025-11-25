using System.Net;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Models;

namespace MeAjudaAi.Integration.Tests.Middleware;

[Collection("Integration")]
public class GeographicRestrictionIntegrationTests : ApiTestBase
{
    [Fact]
    public async Task GetProviders_WhenAllowedCity_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin(); // Authenticate before testing geographic restriction
        Client.DefaultRequestHeaders.Add("X-User-City", "Muriaé");
        Client.DefaultRequestHeaders.Add("X-User-State", "MG");

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Fact]
    public async Task GetProviders_WhenBlockedCity_ShouldReturn451()
    {
        // Arrange
        AuthConfig.ConfigureAdmin(); // Authenticate before testing geographic restriction
        Client.DefaultRequestHeaders.Add("X-User-City", "São Paulo");
        Client.DefaultRequestHeaders.Add("X-User-State", "SP");

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons,
                $"Expected 451 but got {(int)response.StatusCode}. Response: {content}"); // 451

            content.Should().NotBeNullOrWhiteSpace("Response body should not be empty");

            var json = JsonSerializer.Deserialize<JsonElement>(content);

            // Verify all expected fields are present
            json.TryGetProperty("error", out var errorProp).Should().BeTrue($"Missing 'error' property. JSON: {content}");
            json.TryGetProperty("detail", out var detailProp).Should().BeTrue($"Missing 'detail' property. JSON: {content}");
            json.TryGetProperty("allowedCities", out var citiesProp).Should().BeTrue($"Missing 'allowedCities' property. JSON: {content}");
            json.TryGetProperty("yourLocation", out var locationProp).Should().BeTrue($"Missing 'yourLocation' property. JSON: {content}");

            errorProp.GetString().Should().Be("geographic_restriction");
            detailProp.GetString().Should().Contain("Muriaé");
            citiesProp.GetArrayLength().Should().BeGreaterThan(0, "should have at least one allowed city");
            locationProp.TryGetProperty("city", out var cityProp).Should().BeTrue();
            cityProp.GetString().Should().Be("São Paulo");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Fact]
    public async Task GetProviders_WhenAllowedState_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin(); // Authenticate before testing geographic restriction
        Client.DefaultRequestHeaders.Add("X-User-State", "MG");

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Fact]
    public async Task GetProviders_WhenLocationHeader_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin(); // Authenticate before testing geographic restriction
        Client.DefaultRequestHeaders.Add("X-User-Location", "Itaperuna|RJ");

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-Location");
        }
    }

    [Fact]
    public async Task HealthCheck_ShouldAlwaysWork_RegardlessOfLocation()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "Invalid City");
        Client.DefaultRequestHeaders.Add("X-User-State", "XX");

        try
        {
            // Act
            var response = await Client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Fact]
    public async Task Swagger_ShouldAlwaysWork_RegardlessOfLocation()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-User-City", "Invalid City");

        try
        {
            // Act
            var response = await Client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound); // NotFound if swagger disabled
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
        }
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
        AuthConfig.ConfigureAdmin(); // Authenticate before testing geographic restriction
        Client.DefaultRequestHeaders.Add("X-User-City", city);
        Client.DefaultRequestHeaders.Add("X-User-State", state);

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"City '{city}' in state '{state}' should be allowed");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    /// <summary>
    /// Edge case tests for malformed location headers.
    /// These should trigger fail-open behavior (allow access) since location cannot be reliably determined.
    /// </summary>
    [Theory]
    [InlineData("Muriaé|")] // City without state
    [InlineData("|MG")] // State without city
    [InlineData("Muriaé| ")] // City with empty state (spaces)
    [InlineData(" |MG")] // Empty city (spaces) with state
    [InlineData("|")] // Both empty
    [InlineData("  |  ")] // Both empty with spaces
    public async Task GetProviders_WithMalformedLocationHeader_ShouldFailOpen(string malformedLocation)
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", malformedLocation);

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert - malformed entries should be treated as no location (fail-open)
            // Since we can't determine location, middleware allows access
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Malformed location '{malformedLocation}' should be ignored and fail-open");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-Location");
        }
    }

    [Theory]
    [InlineData("")] // Empty city
    [InlineData("  ")] // Only spaces
    [InlineData(null)] // Null city
    public async Task GetProviders_WithEmptyCityHeader_ShouldFailOpen(string? emptyCity)
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        if (emptyCity != null)
        {
            Client.DefaultRequestHeaders.Add("X-User-City", emptyCity);
        }
        Client.DefaultRequestHeaders.Add("X-User-State", "MG");

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert - empty city should fail-open (can't determine location)
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                "Empty city should be treated as undetermined location (fail-open)");
        }
        finally
        {
            if (emptyCity != null)
            {
                Client.DefaultRequestHeaders.Remove("X-User-City");
            }
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Theory]
    [InlineData("")] // Empty state
    [InlineData("  ")] // Only spaces
    public async Task GetProviders_WithEmptyStateHeader_ShouldValidateByCityList(string emptyState)
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-City", "Muriaé");
        Client.DefaultRequestHeaders.Add("X-User-State", emptyState);

        try
        {
            // Act
            var response = await Client.GetAsync("/api/v1/providers");

            // Assert - should validate against city list (Muriaé is allowed)
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                "Valid city with empty state should validate against city list");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove("X-User-City");
            Client.DefaultRequestHeaders.Remove("X-User-State");
        }
    }

    [Fact]
    public async Task GetProviders_WhenNoLocationHeaders_ShouldFailOpen()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        // No location headers added

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert - no location should fail-open (allow access)
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Missing location headers should allow access (fail-open)");
    }
}

