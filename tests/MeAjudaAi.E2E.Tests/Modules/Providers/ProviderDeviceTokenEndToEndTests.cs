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
        // Arrange - Empty token is valid and clears the device token
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
