using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Location;

/// <summary>
/// Integration tests for IBGE service unavailability scenarios.
/// Validates that the geographic restriction middleware properly handles IBGE failures
/// by falling back to simple validation (city/state name matching).
/// </summary>
[Collection("Integration")]
public sealed class IbgeUnavailabilityTests : ApiTestBase, IAsyncLifetime
{
    private WireMockFixture? _wireMockFixture;

    public new async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _wireMockFixture = new WireMockFixture();
        await _wireMockFixture.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        if (_wireMockFixture is not null)
        {
            await _wireMockFixture.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeReturns500_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate IBGE 500 error
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
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
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "itaperuna")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithBody("[]")
                .WithDelay(TimeSpan.FromSeconds(30))); // Exceeds typical timeout

        // Act - Request with Itaperuna (allowed city) should succeed via simple validation
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", "Itaperuna|RJ");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should allow access because Itaperuna is in allowed cities list
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeReturnsMalformedJson_ShouldFallbackToSimpleValidation()
    {
        // Arrange - Configure endpoint to simulate malformed IBGE response
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "linhares")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
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
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "rio de janeiro")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(500));

        // Act - Request with Rio de Janeiro (NOT allowed) should be denied
        AuthConfig.ConfigureAdmin();
        Client.DefaultRequestHeaders.Add("X-User-Location", "Rio de Janeiro|RJ");
        var response = await Client.GetAsync("/api/v1/users");

        // Assert - Should deny access because city is not in allowed list (451 UnavailableForLegalReasons)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task GeographicRestriction_WhenIbgeReturnsEmptyArray_ShouldFallbackToSimpleValidation()
    {
        // Arrange - IBGE returns empty array (city not found)
        _wireMockFixture!.Server
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/v1/localidades/municipios")
                .WithParam("nome", "muriaé")
                .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
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
