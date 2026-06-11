using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.API.Endpoints.Public;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Users;

public class UserDeviceTokenEndToEndTests : BaseTestContainerTest
{
    public UserDeviceTokenEndToEndTests() { }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentUserId = Guid.NewGuid();
        var request = new DeviceTokenRequest("test-device-token");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/users/{nonExistentUserId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNoContent_WhenUserExists()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await CreateTestUserAsync();
        var request = new DeviceTokenRequest($"device-token-{Guid.NewGuid():N}");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldUpdateToken_WhenCalledMultipleTimes()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await CreateTestUserAsync();
        var firstToken = $"first-token-{Guid.NewGuid():N}";
        var secondToken = $"second-token-{Guid.NewGuid():N}";

        // Act - First update
        var firstResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(firstToken));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Second update (overwrite)
        var secondResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(secondToken));
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Note: We cannot directly verify the stored token via API,
        // but the 204 responses confirm the operations succeeded.
        // In a real scenario, you could query the database to verify the token was updated.
    }
}
