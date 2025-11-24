using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Parameterized tests for GeographicRestriction feature flag.
/// Validates behavior when feature is enabled in appsettings.Testing.json.
/// Note: Testing with disabled feature requires separate test class with different environment setup.
/// </summary>
[Collection("Integration")]
public class GeographicRestrictionFeatureFlagTests : ApiTestBase
{
    [Fact]
    public async Task GeographicRestriction_WhenEnabled_ShouldBlockUnauthorizedCities()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", "São Paulo|SP"); // Blocked city

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert - Feature enabled: should block unauthorized locations
        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons,
            "Geographic restriction should block São Paulo when feature is enabled");
    }

    [Fact]
    public async Task GeographicRestriction_WhenEnabled_ShouldOnlyAllowConfiguredCities()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act & Assert - Allowed cities should work
        var allowedCities = new[]
        {
            ("Muriaé", "MG"),
            ("Itaperuna", "RJ"),
            ("Linhares", "ES")
        };

        foreach (var (city, state) in allowedCities)
        {
            Client.DefaultRequestHeaders.Remove("X-User-Location");
            Client.DefaultRequestHeaders.Add("X-User-Location", $"{city}|{state}");

            var response = await Client.GetAsync("/api/v1/providers");

            response.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
                $"{city}/{state} should be allowed when it's in the configured list");
        }

        // Act & Assert - Blocked cities should be denied
        var blockedCities = new[]
        {
            ("São Paulo", "SP"),
            ("Rio de Janeiro", "RJ")
        };

        foreach (var (city, state) in blockedCities)
        {
            Client.DefaultRequestHeaders.Remove("X-User-Location");
            Client.DefaultRequestHeaders.Add("X-User-Location", $"{city}|{state}");

            var response = await Client.GetAsync("/api/v1/providers");

            response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons,
                $"{city}/{state} should be blocked when not in the configured list");
        }
    }

    [Fact]
    public async Task GeographicRestriction_WhenEnabled_ShouldValidateLocationHeaderFormat()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act - Send request without location header
        var responseWithoutHeader = await Client.GetAsync("/api/v1/providers");

        // Send request with malformed location header  
        Client.DefaultRequestHeaders.Add("X-User-Location", "InvalidFormat");
        var responseWithMalformedHeader = await Client.GetAsync("/api/v1/providers");

        // Assert - Should allow when location cannot be determined (fail-open)
        responseWithoutHeader.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
            "Missing location header should fail-open (allow access)");

        responseWithMalformedHeader.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
            "Malformed location header should fail-open (allow access)");
    }
}
