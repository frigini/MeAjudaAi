using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules;

/// <summary>
/// Testes E2E completos para lifecycle de Users, incluindo DELETE com validação de persistência
/// Complementa os testes de autorização existentes com validações de negócio
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Users")]
public class UsersLifecycleE2ETests : TestContainerTestBase
{
    [Fact]
    public async Task DeleteUser_Should_RemoveFromDatabase()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"todelete_{uniqueId}",
            Email = $"todelete_{uniqueId}@example.com",
            FirstName = "ToDelete",
            LastName = "User",
            Password = "Delete@123456"
        };

        // Create user - must succeed for delete test to be meaningful
        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var userId = ExtractIdFromLocation(locationHeader);

        // Act - Delete user
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert - Deletion should return OK or NoContent
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verifica que o usuário não existe mais através da API
        var getAfterDelete = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "User should not exist after deletion");
    }

    [Fact]
    public async Task DeleteUser_NonExistent_Should_Return_NotFound()
    {
        // Arrange
        AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Deleting non-existent user should return NotFound");
    }

    [Fact]
    public async Task DeleteUser_WithoutPermission_Should_Return_ForbiddenOrUnauthorized()
    {
        // Arrange - First create a user as admin
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"testuser_{uniqueId}",
            Email = $"testuser_{uniqueId}@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Test@123456"
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();
        var userId = locationHeader!.Split('/').Last();

        // Now configure a user without delete permission
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-no-delete-123",
            userName: "nodeleteuser",
            email: "nodelete@test.com",
            permissions: [
                Permission.UsersRead.GetValue(),
                Permission.UsersList.GetValue()
            ],
            isSystemAdmin: false,
            roles: []
        );

        // Act - Try to delete the created user without permission
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert - Should get Forbidden/Unauthorized, not NotFound
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized);

        // Cleanup - Delete as admin
        AuthenticateAsAdmin();
        var cleanupResponse = await ApiClient.DeleteAsync($"/api/v1/users/{userId}");
        cleanupResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUserProfile_Should_PersistChanges()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"toupdate_{uniqueId}",
            Email = $"toupdate_{uniqueId}@example.com",
            FirstName = "Original",
            LastName = "Name",
            Password = "Update@123456"
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var userId = ExtractIdFromLocation(locationHeader);

        // Act - Update profile
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "Profile",
            Email = $"updated_{uniqueId}@example.com"
        };

        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, JsonOptions);

        // Assert - Update should return OK or NoContent
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verifica que as mudanças foram persistidas através da API
        var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Updated");
        content.Should().Contain("Profile");
    }

    [Fact]
    public async Task GetUserByEmail_Should_ReturnCorrectUser()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var uniqueEmail = $"byemail_{uniqueId}@example.com";

        var createRequest = new
        {
            Username = $"byemail_{uniqueId}",
            Email = uniqueEmail,
            FirstName = "ByEmail",
            LastName = "Test",
            Password = "Email@123456"
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var getByEmailResponse = await ApiClient.GetAsync($"/api/v1/users/by-email/{Uri.EscapeDataString(uniqueEmail)}");

        // Assert
        getByEmailResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "User should be found by email after creation");

        var content = await getByEmailResponse.Content.ReadAsStringAsync();
        content.Should().Contain(uniqueEmail,
            "Response should contain the queried email");
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Should_Fail()
    {
        // Arrange
        AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sharedEmail = $"duplicate_{uniqueId}@example.com";

        var firstUserRequest = new
        {
            Username = $"first_{uniqueId}",
            Email = sharedEmail,
            FirstName = "First",
            LastName = "User",
            Password = "First@123456"
        };

        var firstResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", firstUserRequest, JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Tenta criar segundo usuário com mesmo email
        var secondUserRequest = new
        {
            Username = $"second_{uniqueId}",
            Email = sharedEmail, // Email duplicado
            FirstName = "Second",
            LastName = "User",
            Password = "Second@123456"
        };

        var secondResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", secondUserRequest, JsonOptions);

        // Assert
        secondResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            HttpStatusCode.UnprocessableEntity);
    }
}
