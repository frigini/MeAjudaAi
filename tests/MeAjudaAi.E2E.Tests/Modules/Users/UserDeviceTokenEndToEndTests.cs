using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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
    public async Task Put_DeviceToken_ShouldPersistToken()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await CreateTestUserAsync();
        var token = $"device-token-{Guid.NewGuid():N}";

        // Act - Set token
        var setResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(token));
        setResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get user to verify persistence
        var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userJson = await getResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<JsonElement>(userJson, JsonOptions);
        var userData = GetResponseData(userResponse);

        // Assert - Token was persisted
        var deviceToken = userData.GetProperty("deviceToken").GetString();
        deviceToken.Should().Be(token, "device token should be persisted after update");
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReplacePreviousToken()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await CreateTestUserAsync();
        var firstToken = $"first-token-{Guid.NewGuid():N}";
        var secondToken = $"second-token-{Guid.NewGuid():N}";

        // Act - Set first token
        var firstResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(firstToken));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Replace with second token
        var secondResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(secondToken));
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get user to verify replacement
        var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userJson = await getResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<JsonElement>(userJson, JsonOptions);
        var userData = GetResponseData(userResponse);

        // Assert - Second token replaced the first
        var deviceToken = userData.GetProperty("deviceToken").GetString();
        deviceToken.Should().Be(secondToken, "second update should replace the first token");
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldClearToken_WhenEmpty()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await CreateTestUserAsync();
        var token = $"device-token-{Guid.NewGuid():N}";

        // Act - Set token
        var setResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(token));
        setResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify token was set
        var getResponse1 = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        var userResponse1 = JsonSerializer.Deserialize<JsonElement>(
            await getResponse1.Content.ReadAsStringAsync(), JsonOptions);
        var user1 = GetResponseData(userResponse1);
        user1.GetProperty("deviceToken").GetString().Should().Be(token);

        // Act - Clear token with empty string
        var clearResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/device-token",
            new DeviceTokenRequest(""));
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get user to verify token was cleared
        var getResponse2 = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var userResponse2 = JsonSerializer.Deserialize<JsonElement>(
            await getResponse2.Content.ReadAsStringAsync(), JsonOptions);
        var user2 = GetResponseData(userResponse2);

        // Assert - Token should be null/empty after clearing
        var deviceToken = user2.GetProperty("deviceToken");
        (deviceToken.ValueKind == JsonValueKind.Null || 
         string.IsNullOrEmpty(deviceToken.GetString())).Should().BeTrue(
            "empty string should clear the device token");
    }
}
