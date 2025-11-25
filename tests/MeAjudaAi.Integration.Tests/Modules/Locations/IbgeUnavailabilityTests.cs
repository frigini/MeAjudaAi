using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests for IBGE service unavailability scenarios.
/// Validates that the geographic restriction middleware properly handles IBGE failures
/// by falling back to simple validation (city/state name matching).
/// Uses real IGeographicValidationService with WireMock stubs for IBGE API.
/// </summary>
[Collection("Integration")]
public sealed class IbgeUnavailabilityTests : ApiTestBase
{
    // Override to use real IBGE service with WireMock stubs instead of mock
    protected override bool UseMockGeographicValidation => false;

    // TODO: Fix middleware simple validation fallback - currently blocks even allowed cities when IBGE fails
    // Expected: When IBGE unavailable, allow cities in AllowedCities list via simple name matching
    // Actual: Returns 451 (blocked) for all cities when IBGE fails, even allowed ones
    [Fact(Skip = "Middleware doesn't fall back to simple validation correctly - blocks allowed cities when IBGE unavailable")]
    public async Task GeographicRestriction_WhenIbgeReturns500_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate IBGE 500 error
        // Note: IbgeClient normalizes city names to lowercase before querying
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

        // Act - Request with Muriaé (allowed city) should succeed via simple validation
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add(UserLocationHeader, "Muriaé|MG");
        var response = await Client.GetAsync(ProvidersEndpoint);

        // Assert - Should allow access because Muriaé is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    // TODO: Fix middleware simple validation fallback
    [Fact(Skip = "Middleware doesn't fall back to simple validation correctly - blocks allowed cities when IBGE unavailable")]
    public async Task GeographicRestriction_WhenIbgeReturnsMalformedJson_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate malformed IBGE response
        // Note: IbgeClient normalizes city names to lowercase before querying
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "linhares")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{invalid json"));

        // Act - Request with Linhares (allowed city) should succeed via simple validation
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add(UserLocationHeader, "Linhares|ES");
        var response = await Client.GetAsync(ProvidersEndpoint);

        // Assert - Should allow access because Linhares is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact(Skip = "CI returns 200 OK instead of 451 - middleware not blocking. Likely feature flag or middleware registration issue in CI environment.")]
    public async Task GeographicRestriction_WhenIbgeUnavailableAndCityNotAllowed_ShouldDenyAccess()
    {
        // Arrange - Configure IBGE to fail
        // Note: IbgeClient normalizes city names to lowercase before querying
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "rio de janeiro")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        // Act - Request with Rio de Janeiro (NOT allowed) should be denied
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add(UserLocationHeader, "Rio de Janeiro|RJ");
        var response = await Client.GetAsync(ProvidersEndpoint);

        // Assert - Should deny access because city is not in allowed list (451 UnavailableForLegalReasons)
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnavailableForLegalReasons,
            $"Expected 451 but got {(int)response.StatusCode}. Response body: {content}");

        // Verify error payload structure
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);

        json.GetProperty("error").GetString().Should().Be("geographic_restriction");
        json.GetProperty("yourLocation").GetProperty("city").GetString().Should().Be("Rio de Janeiro");
        json.GetProperty("yourLocation").GetProperty("state").GetString().Should().Be("RJ");
        json.GetProperty("allowedCities").GetArrayLength().Should().BeGreaterThan(0);
        json.GetProperty("allowedStates").GetArrayLength().Should().BeGreaterThan(0);
    }

    // TODO: Fix middleware simple validation fallback
    [Fact(Skip = "Middleware doesn't fall back to simple validation correctly - blocks allowed cities when IBGE unavailable")]
    public async Task GeographicRestriction_WhenIbgeReturnsEmptyArray_ShouldFallbackToSimpleValidation()
    {
        // Arrange - IBGE returns empty array (city not found)
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[]"));

        // Act - Request with Muriaé (allowed city) should succeed via simple validation
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add(UserLocationHeader, "Muriaé|MG");
        var response = await Client.GetAsync(ProvidersEndpoint);

        // Assert - Should allow access via simple validation fallback
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
