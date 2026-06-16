using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

public class ProviderDeviceTokenEndToEndTests : BaseTestContainerTest
{
    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();
        var request = new ProviderDeviceTokenRequest("test-device-token");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnNotFound_WhenProviderDoesNotExist_WithEmptyToken()
    {
        // Arrange - Provider doesn't exist, expect NotFound regardless of token content
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();
        var request = new ProviderDeviceTokenRequest("");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/device-token", request);

        // Assert - Provider doesn't exist, so returns NotFound
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReturnOk_WhenProviderExists()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        var request = new ProviderDeviceTokenRequest("valid-device-token-12345");

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/device-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldPersistToken()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        var token = $"device-token-{Guid.NewGuid():N}";

        // Act - Set token
        var setResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/device-token",
            new ProviderDeviceTokenRequest(token));
        setResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get provider to verify persistence
        var getResponse = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var providerJson = await getResponse.Content.ReadAsStringAsync();
        var providerResponse = JsonSerializer.Deserialize<JsonElement>(providerJson, JsonOptions);
        var providerData = GetResponseData(providerResponse);

        // Assert - Token was persisted
        var deviceToken = providerData.GetProperty("deviceToken").GetString();
        deviceToken.Should().Be(token, "device token should be persisted after update");
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldReplacePreviousToken()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        var firstToken = $"first-token-{Guid.NewGuid():N}";
        var secondToken = $"second-token-{Guid.NewGuid():N}";

        // Act - Set first token
        var firstResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/device-token",
            new ProviderDeviceTokenRequest(firstToken));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Replace with second token
        var secondResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/device-token",
            new ProviderDeviceTokenRequest(secondToken));
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get provider to verify replacement
        var getResponse = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var providerJson = await getResponse.Content.ReadAsStringAsync();
        var providerResponse = JsonSerializer.Deserialize<JsonElement>(providerJson, JsonOptions);
        var providerData = GetResponseData(providerResponse);

        // Assert - Second token replaced the first
        var deviceToken = providerData.GetProperty("deviceToken").GetString();
        deviceToken.Should().Be(secondToken, "second update should replace the first token");
    }

    [Fact]
    public async Task Put_DeviceToken_ShouldClearToken_WhenEmpty()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        var token = $"device-token-{Guid.NewGuid():N}";

        // Act - Set token
        var setResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/device-token",
            new ProviderDeviceTokenRequest(token));
        setResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify token was set
        var getResponse1 = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
        var providerResponse1 = JsonSerializer.Deserialize<JsonElement>(
            await getResponse1.Content.ReadAsStringAsync(), JsonOptions);
        var provider1 = GetResponseData(providerResponse1);
        provider1.GetProperty("deviceToken").GetString().Should().Be(token);

        // Act - Clear token with empty string
        var clearResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/device-token",
            new ProviderDeviceTokenRequest(""));
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Get provider to verify token was cleared
        var getResponse2 = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
        getResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var providerResponse2 = JsonSerializer.Deserialize<JsonElement>(
            await getResponse2.Content.ReadAsStringAsync(), JsonOptions);
        var provider2 = GetResponseData(providerResponse2);

        // Assert - Token should be null/empty after clearing
        var deviceToken = provider2.GetProperty("deviceToken");
        (deviceToken.ValueKind == JsonValueKind.Null || 
         string.IsNullOrEmpty(deviceToken.GetString())).Should().BeTrue(
            "empty string should clear the device token");
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        var userId = await CreateTestUserAsync();
        var providerName = $"Test Provider {Guid.NewGuid():N}";

        var request = new
        {
            UserId = userId.ToString(),
            Name = providerName,
            Type = 0,
            BusinessProfile = new
            {
                LegalName = providerName,
                FantasyName = providerName,
                Description = "Test provider",
                ContactInfo = new
                {
                    Email = $"provider_{Guid.NewGuid():N}@example.com",
                    PhoneNumber = "+5511999999999",
                    Website = "https://www.example.com"
                },
                PrimaryAddress = new
                {
                    Street = "Test Street",
                    Number = "123",
                    Complement = (string?)null,
                    Neighborhood = "Test Neighborhood",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", request, JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create provider. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            throw new InvalidOperationException("Location header not found in create provider response");
        }

        return ExtractIdFromLocation(location);
    }
}
