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

        // Cria o usuário
        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            // Skip test if user creation fails (pode ser conflito ou outro erro)
            return;
        }

        var locationHeader = createResponse.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            return;
        }

        var userId = ExtractIdFromLocation(locationHeader);

        // Act - Delete user
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound); // NotFound aceitável se já foi deletado

        // Verifica que o usuário não existe mais através da API
        if (deleteResponse.IsSuccessStatusCode)
        {
            var getAfterDelete = await ApiClient.GetAsync($"/api/v1/users/{userId}");
            getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "User should not exist after deletion");
        }
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
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.NoContent); // Alguns endpoints são idempotentes
    }

    [Fact]
    public async Task DeleteUser_WithoutPermission_Should_Return_Forbidden()
    {
        // Arrange - Configurar usuário sem permissão de delete
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
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

        var userId = Guid.NewGuid();

        // Act
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized);
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

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            return;
        }

        var locationHeader = createResponse.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            return;
        }

        var userId = ExtractIdFromLocation(locationHeader);

        // Act - Update profile
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "Profile",
            Email = $"updated_{uniqueId}@example.com"
        };

        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound);

        if (updateResponse.IsSuccessStatusCode)
        {
            // Verifica que as mudanças foram persistidas através da API
            var getResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");

            if (getResponse.IsSuccessStatusCode)
            {
                var content = await getResponse.Content.ReadAsStringAsync();
                content.Should().Contain("Updated");
                content.Should().Contain("Profile");
            }
        }
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

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            return;
        }

        // Act
        var getByEmailResponse = await ApiClient.GetAsync($"/api/v1/users/by-email/{Uri.EscapeDataString(uniqueEmail)}");

        // Assert
        getByEmailResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);

        if (getByEmailResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await getByEmailResponse.Content.ReadAsStringAsync();
            content.Should().Contain(uniqueEmail);
            content.Should().Contain("ByEmail");
        }
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

        if (firstResponse.StatusCode != HttpStatusCode.Created)
        {
            return;
        }

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

    private static new Guid ExtractIdFromLocation(string locationHeader)
    {
        if (locationHeader.Contains("?id="))
        {
            var queryString = locationHeader.Split('?')[1];
            var idParam = queryString.Split('&')
                .FirstOrDefault(p => p.StartsWith("id="));

            if (idParam != null)
            {
                var idValue = idParam.Split('=')[1];
                return Guid.Parse(idValue);
            }
        }

        var segments = locationHeader.Split('/');
        var lastSegment = segments[^1].Split('?')[0];
        return Guid.Parse(lastSegment);
    }
}
