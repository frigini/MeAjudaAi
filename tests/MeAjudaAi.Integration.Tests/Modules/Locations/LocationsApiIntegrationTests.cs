using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Integration.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

public class LocationsApiIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Locations;

    [Fact]
    public async Task SearchLocations_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var query = "Muriae";

        // Act
        var response = await Client.GetAsync($"/api/v1/locations/search?query={query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<LocationCandidate>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchLocations_WithShortQuery_ShouldReturnEmptyList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var query = "Mu";

        // Act
        var response = await Client.GetAsync($"/api/v1/locations/search?query={query}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<LocationCandidate>>();
        result.Should().BeEmpty();
    }
}
