using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Utilities.Constants;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProviderDashboardApiTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public ProviderDashboardApiTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMyProfile_Should_Return_Correct_Provider()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var userId = await _fixture.CreateTestUserAsync();
        var providerId = await CreateTestProviderForUserAsync(userId);

        // Act - Switch to Provider User
        TestContainerFixture.AuthenticateAsUser(userId.ToString());
        
        var response = await _fixture.ApiClient.GetAsync("/api/v1/providers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        // Handle Result wrapper if present
        JsonElement provider;
        if (root.TryGetProperty("value", out var valueProp))
        {
            provider = valueProp;
        }
        else 
        {
            provider = root;
        }

        provider.GetProperty("id").GetString().Should().Be(providerId.ToString());
        provider.GetProperty("userId").GetString().Should().Be(userId.ToString());
    }

    [Fact]
    public async Task UpdateMyProfile_Description_Should_Update_Successfully()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var userId = await _fixture.CreateTestUserAsync();
        var providerId = await CreateTestProviderForUserAsync(userId);
        
        // Act - Switch to Provider User
        TestContainerFixture.AuthenticateAsUser(userId.ToString());
        
        // Get current profile to have full object (since PUT requires full object typically, or at least required fields)
        // My endpoint uses UpdateProviderProfileRequest which has Name + BusinessProfile.
        var getResponse = await _fixture.ApiClient.GetAsync("/api/v1/providers/me");
        var content = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        JsonElement provider = root.TryGetProperty("value", out var v) ? v : root;

        var newDescription = "Updated Description via Dashboard";
        
        var updateRequest = new
        {
             Name = provider.GetProperty("name").GetString(),
             BusinessProfile = new
             {
                 LegalName = provider.GetProperty("businessProfile").GetProperty("legalName").GetString(),
                 FantasyName = provider.GetProperty("businessProfile").GetProperty("fantasyName").GetString(),
                 Description = newDescription,
                 ContactInfo = new
                 {
                     Email = provider.GetProperty("businessProfile").GetProperty("contactInfo").GetProperty("email").GetString(),
                     PhoneNumber = provider.GetProperty("businessProfile").GetProperty("contactInfo").GetProperty("phoneNumber").GetString(),
                     Website = provider.GetProperty("businessProfile").GetProperty("contactInfo").TryGetProperty("website", out var w) ? w.GetString() : null
                 },
                 PrimaryAddress = new
                 {
                     Street = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("street").GetString(),
                     Number = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("number").GetString(),
                     Neighborhood = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("neighborhood").GetString(),
                     City = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("city").GetString(),
                     State = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("state").GetString(),
                     ZipCode = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("zipCode").GetString(),
                     Country = provider.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("country").GetString()
                 }
             }
        };

        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync("/api/v1/providers/me", updateRequest, TestContainerFixture.JsonOptions);

        // Assert
        updateResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var verifyResponse = await _fixture.ApiClient.GetAsync("/api/v1/providers/me");
        var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
        using var verifyDoc = JsonDocument.Parse(verifyContent);
        var verifyRoot = verifyDoc.RootElement;
        JsonElement verifyProvider = verifyRoot.TryGetProperty("value", out var vp) ? vp : verifyRoot;
        
        verifyProvider.GetProperty("businessProfile").GetProperty("description").GetString().Should().Be(newDescription);
    }
    
    // Services Management
    // For services, we normally need a ServiceId. 
    // In E2E, we need to Create a ServiceCatalog item first?
    // Providers Module depends on ServiceCatalogs?
    // If ServiceCatalogs module is initialized, we can create a service.
    // I need to check if I can create a service via API or seed.
    // Assuming I can't easily create a service in this test without ServiceCatalogs API client.
    // But AppHost has ServiceCatalogs module.
    // I'll skip AddService test complexity for now or try to create one if endpoint is known.
    // E2E test project references ServiceCatalogs module.
    
    private async Task<Guid> CreateTestProviderForUserAsync(Guid userId)
    {
        var providerName = _fixture.Faker.Company.CompanyName();

        var request = new
        {
            UserId = userId.ToString(),
            Name = providerName,
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = providerName,
                FantasyName = providerName,
                Description = $"Test provider {providerName}",
                ContactInfo = new
                {
                    Email = _fixture.Faker.Internet.Email(),
                    PhoneNumber = "+5511999999999",
                    Website = "https://www.example.com"
                },
                PrimaryAddress = new
                {
                    Street = _fixture.Faker.Address.StreetAddress(),
                    Number = _fixture.Faker.Random.Number(1, 9999).ToString(),
                    Complement = (string?)null,
                    Neighborhood = _fixture.Faker.Address.City(),
                    City = _fixture.Faker.Address.City(),
                    State = _fixture.Faker.Address.StateAbbr(),
                    ZipCode = _fixture.Faker.Address.ZipCode(),
                    Country = "Brasil"
                }
            }
        };

        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request, TestContainerFixture.JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create provider. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var location = response.Headers.Location?.ToString();
        return TestContainerFixture.ExtractIdFromLocation(location!);
    }
}
