using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Testes de integração para cenários de indisponibilidade do serviço IBGE.
/// Valida que o middleware de restrição geográfica trata corretamente falhas do IBGE
/// fazendo fallback para validação simples (correspondência de nome de cidade/estado).
/// Usa IGeographicValidationService real com stubs WireMock para a API do IBGE.
/// </summary>
[Collection("Integration")]
public sealed class IbgeUnavailabilityTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations | TestModule.Providers;

    // Override to use real IBGE service with WireMock stubs instead of mock
    protected override bool UseMockGeographicValidation => false;

    [Fact]
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

    [Fact]
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

    // INVESTIGATION RESULTS:
    // - Feature flag is correctly configured: FeatureManagement:GeographicRestriction = true
    // - Middleware logic is correct: checks IsEnabledAsync before blocking
    // - Test configuration in BaseApiTest sets GeographicRestriction = true
    // - CI environment may have different configuration overriding test settings
    // - Possible causes:
    //   1. appsettings.Testing.json not being loaded in CI
    //   2. Environment-specific config (ASPNETCORE_ENVIRONMENT) different in CI
    //   3. Feature flag provider (Microsoft.FeatureManagement) initialization issue in CI
    // - SOLUTION: Add explicit feature flag validation in test setup to fail fast if misconfigured
    //   rather than skipping the test. This will surface configuration issues immediately.
    [Fact]
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
        Client.DefaultRequestHeaders.Add(UserLocationHeader, "Muriaé|MG");
        var response = await Client.GetAsync(ProvidersEndpoint);

        // Assert - Should allow access via simple validation fallback
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
