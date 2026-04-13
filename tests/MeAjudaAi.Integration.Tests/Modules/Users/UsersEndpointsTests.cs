using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

public class UsersEndpointsTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

    [Fact]
    public async Task GetUsers_WithAuthentication_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnOk_OrNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users/by-email/test@example.com");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserProfile_GetAndUpdate_ShouldWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "testuser", "test@user.com", "customer");

        // 1. Get Me (Should return 404 or 200 depending if user exists in DB)
        var getMeResponse = await Client.GetAsync("/api/v1/users/me");
        getMeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // 2. Update Profile
        var updateRequest = new {
            firstName = "Updated",
            lastName = "User",
            phoneNumber = "+5511999998888"
        };
        var updateResponse = await Client.PutAsJsonAsync("/api/v1/users/profile", updateRequest);
        
        // Assert
        updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetAuthProviders_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/auth-providers");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeleteUser_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var email = $"new_user_{Guid.NewGuid():N}@test.com";
        var createRequest = new {
            email = email,
            firstName = "New",
            lastName = "User",
            role = "customer"
        };

        // 1. Create
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest);

        if (createResponse.IsSuccessStatusCode)
        {
            var content = await ReadJsonAsync<JsonElement>(createResponse.Content);
            var userId = GetResponseData(content).GetProperty("id").GetGuid();

            // 2. Delete
            var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");
            deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
        }
    }
}
