using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

public class ProviderDeviceTokenEndToEndTests : BaseTestContainerTest
{
    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new ProviderDeviceTokenRequest("test-device-token");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnBadRequest_WhenTokenIsEmpty()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new ProviderDeviceTokenRequest("");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
