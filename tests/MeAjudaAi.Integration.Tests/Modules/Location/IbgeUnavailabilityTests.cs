using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

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

    [Fact]
    public async Task GeographicRestriction_WhenIbgeReturns500_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate IBGE 500 error
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
        Client.DefaultRequestHeaders.Add("X-User-Location", "Muriaé|MG");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should allow access because Muriaé is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeTimesOut_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate IBGE timeout
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "itaperuna")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("[]")
                .WithDelay(TimeSpan.FromSeconds(30))); // Exceeds typical timeout

        // Act - Request with Itaperuna (allowed city) should succeed via simple validation
        AuthConfig.ConfigureAdmin();
        Client.Timeout = TimeSpan.FromSeconds(10); // Set timeout lower than WireMock delay to trigger timeout
        Client.DefaultRequestHeaders.Add("X-User-Location", "Itaperuna|RJ");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should allow access because Itaperuna is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeReturnsMalformedJson_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate malformed IBGE response
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
        Client.DefaultRequestHeaders.Add("X-User-Location", "Linhares|ES");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should allow access because Linhares is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeUnavailableAndCityNotAllowed_ShouldDenyAccess()
    {
        // Arrange - Configure IBGE to fail
        WireMock.Server
            .Given(global::WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "rio de janeiro")
                .UsingGet())
            .RespondWith(global::WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        // Act - Request with Rio de Janeiro (NOT allowed) should be denied
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", "Rio de Janeiro|RJ");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should deny access because city is not in allowed list (451 UnavailableForLegalReasons)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnavailableForLegalReasons);

        // Verify error payload structure
        var content = await response.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);

        json.GetProperty("error").GetString().Should().Be("geographic_restriction");
        json.GetProperty("yourLocation").GetProperty("city").GetString().Should().Be("Rio de Janeiro");
        json.GetProperty("yourLocation").GetProperty("state").GetString().Should().Be("RJ");
        json.GetProperty("allowedCities").GetArrayLength().Should().BeGreaterThan(0);
        json.GetProperty("allowedStates").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
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
        Client.DefaultRequestHeaders.Add("X-User-Location", "Muriaé|MG");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should allow access via simple validation fallback
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
