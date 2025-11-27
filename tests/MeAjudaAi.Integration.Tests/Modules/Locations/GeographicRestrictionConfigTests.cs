using System.Net;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Tests to diagnose GeographicRestriction feature flag configuration issues in CI.
/// These tests validate that the middleware is properly configured and blocking requests.
/// If these tests fail, it indicates a middleware registration or configuration issue.
/// </summary>
[Collection("Integration")]
public sealed class GeographicRestrictionConfigTests : ApiTestBase
{
    protected override bool UseMockGeographicValidation => false;

    [Fact]
    public async Task GeographicRestriction_ShouldBlock_WhenCityNotInAllowedList()
    {
        // Arrange: Non-allowed city (São Paulo not in allowed list: MG, RJ, ES only)
        Client.DefaultRequestHeaders.Add("X-User-Location", "São Paulo|SP");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert: Should be blocked with 451
        Assert.Equal(HttpStatusCode.UnavailableForLegalReasons, response.StatusCode);
    }

    [Fact]
    public async Task GeographicRestriction_ShouldAllow_WhenCityInAllowedList()
    {
        // Arrange: Allowed city (Muriaé is in allowed list)
        Client.DefaultRequestHeaders.Add("X-User-Location", "Muriaé|MG");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert: Should pass through middleware (401 Unauthorized because no auth, NOT 451)
        Assert.NotEqual(HttpStatusCode.UnavailableForLegalReasons, response.StatusCode);
    }

    [Fact]
    public async Task GeographicRestriction_ShouldBlock_WhenStateNotInAllowedList()
    {
        // Arrange: Non-allowed state (BA not in allowed states: MG, RJ, ES only)
        Client.DefaultRequestHeaders.Add("X-User-Location", "Salvador|BA");

        // Act
        var response = await Client.GetAsync("/api/v1/providers");

        // Assert: Should be blocked with 451
        Assert.Equal(HttpStatusCode.UnavailableForLegalReasons, response.StatusCode);
    }
}
