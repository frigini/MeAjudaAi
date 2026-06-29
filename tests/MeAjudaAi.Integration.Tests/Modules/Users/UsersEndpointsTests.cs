using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

public class UsersEndpointsTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

    [Fact]
    public async Task GetUsers_WithAdminAuth_ShouldReturnOk()
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
    public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users/by-email/nonexistent@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuthProviders_ShouldReturnOk()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/auth/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAndDeleteUser_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var email = $"new_user_{Guid.NewGuid():N}@test.com";
        var createRequest = new
        {
            username = $"user_{Guid.NewGuid():N}"[..20],
            email = email,
            firstName = "New",
            lastName = "User",
            password = "Password123!",
            keycloakId = Guid.NewGuid().ToString()
        };

        // 1. Create
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var userId = GetResponseData(content).GetProperty("id").GetGuid();

        // 2. Delete
        var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUserProfile_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "User"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/users/{nonExistentId}/profile", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
