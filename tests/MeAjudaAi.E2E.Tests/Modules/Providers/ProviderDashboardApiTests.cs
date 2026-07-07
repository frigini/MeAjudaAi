using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProviderDashboardApiTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{
    [Fact]
    public async Task GetMyProfile_Should_Return_Correct_Provider()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();

        var userId = await Fixture.CreateTestUserAsync();
        var providerId = await Fixture.CreateTestProviderAsync(userId);

        // Act - Switch to Provider User
        TestContainerFixture.AuthenticateAsUser(userId.ToString());

        var response = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var provider = await TestContainerFixture.ReadJsonAsync<JsonElement>(response);
        var value = provider.TryGetProperty("data", out var v) ? v : provider;

        value.GetProperty("id").GetString().Should().Be(providerId.ToString());
        value.GetProperty("userId").GetString().Should().Be(userId.ToString());
    }

    [Fact]
    public async Task UpdateMyProfile_Description_Should_Update_Successfully()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();

        var userId = await Fixture.CreateTestUserAsync();
        _ = await Fixture.CreateTestProviderAsync(userId);

        // Act - Switch to Provider User
        TestContainerFixture.AuthenticateAsUser(userId.ToString());

        // Get current profile
        var getResponse = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var provider = await TestContainerFixture.ReadJsonAsync<JsonElement>(getResponse);
        var value = provider.TryGetProperty("data", out var v) ? v : provider;

        var newDescription = "Updated Description via Dashboard";

        var updateRequest = new
        {
            Name = value.GetProperty("name").GetString(),
            BusinessProfile = new
            {
                LegalName = value.GetProperty("businessProfile").GetProperty("legalName").GetString(),
                FantasyName = value.GetProperty("businessProfile").GetProperty("fantasyName").GetString(),
                Description = newDescription,
                ContactInfo = new
                {
                    Email = value.GetProperty("businessProfile").GetProperty("contactInfo").GetProperty("email").GetString(),
                    PhoneNumber = value.GetProperty("businessProfile").GetProperty("contactInfo").GetProperty("phoneNumber").GetString(),
                    Website = value.GetProperty("businessProfile").GetProperty("contactInfo").TryGetProperty("website", out var w) ? w.GetString() : null
                },
                PrimaryAddress = new
                {
                    Street = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("street").GetString(),
                    Number = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("number").GetString(),
                    Neighborhood = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("neighborhood").GetString(),
                    City = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("city").GetString(),
                    State = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("state").GetString(),
                    ZipCode = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("zipCode").GetString(),
                    Country = value.GetProperty("businessProfile").GetProperty("primaryAddress").GetProperty("country").GetString()
                }
            }
        };

        var updateResponse = await Fixture.PutJsonAsync("/api/v1/providers/me", updateRequest);

        // Assert
        updateResponse.IsSuccessStatusCode.Should().BeTrue();

        var verifyResponse = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        var verifyProvider = await TestContainerFixture.ReadJsonAsync<JsonElement>(verifyResponse);
        var verifyValue = verifyProvider.TryGetProperty("data", out var vp) ? vp : verifyProvider;

        verifyValue.GetProperty("businessProfile").GetProperty("description").GetString().Should().Be(newDescription);
    }
}
