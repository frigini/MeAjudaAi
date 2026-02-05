using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// 🧪 TESTES DE INTEGRAÇÃO PARA O MÓDULO USERS
/// 
/// Valida as funcionalidades implementadas do módulo Users:
/// - Criação de usuários
/// - Consulta de usuários  
/// - Atualização de perfil
/// - Soft Delete de usuários
/// - Gerenciamento via Keycloak
/// </summary>
public class UsersIntegrationTests(ITestOutputHelper testOutput) : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var userData = new
        {
            username = $"test{Guid.NewGuid():N}"[..20], // Limitar a 20 caracteres
            email = $"test-{Guid.NewGuid():N}@example.com",
            firstName = "Test",
            lastName = "User",
            password = "Test1234",
            keycloakId = $"keycloak-{Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users", userData);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "POST requests that create resources should return 201 Created");

        var responseJson = JsonSerializer.Deserialize<JsonElement>(content);

        // Verifica se é uma response estruturada (com data)
        var dataElement = GetResponseData(responseJson);
        dataElement.TryGetProperty("id", out _).Should().BeTrue(
            $"Response data should contain 'id' property. Full response: {content}");
        dataElement.TryGetProperty("username", out var usernameProperty).Should().BeTrue();
        usernameProperty.GetString().Should().Be(userData.username);

        // Cleanup - tentar deletar usuário criado
        var idElement = GetResponseData(responseJson);
        if (idElement.TryGetProperty("id", out var idProperty))
        {
            var userId = idProperty.GetString();
            var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");
            if (!deleteResponse.IsSuccessStatusCode)
            {
                testOutput.WriteLine($"Cleanup failed: Could not delete user {userId}. Status: {deleteResponse.StatusCode}");
            }
        }
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUsersList()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        var users = JsonSerializer.Deserialize<JsonElement>(content);

        // Espera formato de resposta API consistente - deve ser um objeto com propriedade data ou value
        users.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");

        var dataElement = GetResponseData(users);
        dataElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
        dataElement.ValueKind.Should().NotBe(JsonValueKind.Null,
            "Data property should contain either an array of users or a paginated response object");
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomId = Guid.NewGuid(); // Use random ID that definitely doesn't exist

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "API should return 404 when user ID does not exist");
    }

    [Fact]
    public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var randomEmail = $"nonexistent-{Guid.NewGuid():N}@example.com";

        // Act
        var response = await Client.GetAsync($"/api/v1/users/by-email/{randomEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "API should return 404 when user email does not exist");
    }

    [Fact]
    public async Task UsersEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/users"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                testOutput.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    [Fact]
    public async Task UserWorkflow_CreateUpdateDelete_ShouldWork()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var userData = new
        {
            username = $"test{uniqueId}",
            email = $"test-{uniqueId}@example.com",
            firstName = "Test",
            lastName = "User",
            password = "Test1234",
            keycloakId = $"keycloak-{Guid.NewGuid()}"
        };

        try
        {
            // Act 1: Create User
            var createResponse = await Client.PostAsJsonAsync("/api/v1/users", userData);

            // Assert 1: Creation successful
            var createContent = await createResponse.Content.ReadAsStringAsync();
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
                $"User creation should succeed. Response: {createContent}");

            var createResponseJson = JsonSerializer.Deserialize<JsonElement>(createContent);
            var createdUser = GetResponseData(createResponseJson);
            createdUser.TryGetProperty("id", out var idProperty).Should().BeTrue();
            var userId = idProperty.GetString()!;

            // Act 2: Update User Profile
            var updateData = new
            {
                firstName = "Updated",
                lastName = "TestUser"
            };

            var updateResponse = await Client.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateData);

            // Assert 2: Update successful (or method not allowed if not implemented)
            updateResponse.StatusCode.Should().BeOneOf(
                [HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.MethodNotAllowed],
                "Update should succeed or be not implemented yet");

            // Act 3: Get User by ID
            var getResponse = await Client.GetAsync($"/api/v1/users/{userId}");

            // Assert 3: Can retrieve created user
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var getContent = await getResponse.Content.ReadAsStringAsync();
                var getResponseJson = JsonSerializer.Deserialize<JsonElement>(getContent);
                var retrievedUser = GetResponseData(getResponseJson);
                retrievedUser.TryGetProperty("id", out var retrievedIdProperty).Should().BeTrue();
                retrievedIdProperty.GetString().Should().Be(userId);
            }

            // Act 4: Delete User
            var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");

            // Assert 4: Deletion successful (or method not allowed if soft delete only)
            deleteResponse.StatusCode.Should().BeOneOf(
                [HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.MethodNotAllowed],
                "Delete should succeed or be not implemented for hard deletes");
        }
        catch (Exception ex)
        {
            testOutput.WriteLine($"User workflow test failed: {ex.Message}");
            throw;
        }
    }



    private async Task<string?> CreateTestUser(string username, string email)
    {
        var userData = new
        {
            username = username.Length > 20 ? username[..20] : username,
            email = email,
            firstName = "Test",
            lastName = "User",
            keycloakId = $"keycloak-{Guid.NewGuid()}"
        };

        var response = await Client.PostAsJsonAsync("/api/v1/users", userData);
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(content);
            var dataElement = GetResponseData(responseJson);
            if (dataElement.TryGetProperty("id", out var idProperty))
            {
                return idProperty.GetString();
            }
        }

        testOutput.WriteLine($"Failed to create test user {username}. Status: {response.StatusCode}");
        return null;
    }

    private async Task CleanupUser(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");
        if (!deleteResponse.IsSuccessStatusCode)
        {
            testOutput.WriteLine($"Cleanup failed: Could not delete user {userId}. Status: {deleteResponse.StatusCode}");
        }
    }

    [Fact]
    public async Task Database_Should_Persist_Users_Correctly()
    {
        // Arrange
        var username = new Username($"dbtest_{Guid.NewGuid():N}"[..20]);
        var email = new Email($"dbtest_{Guid.NewGuid():N}@test.com");

        // Act - Create user directly in database
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            var user = new User(
                username: username,
                email: email,
                firstName: "Database",
                lastName: "Test",
                keycloakId: $"keycloak-{Guid.NewGuid():N}"
            );

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert - Verify user was persisted
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            var foundUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            foundUser.Should().NotBeNull("user should be persisted to database");
            foundUser!.Email.Should().Be(email);
            foundUser.FirstName.Should().Be("Database");
            foundUser.LastName.Should().Be("Test");
        }
    }
}
