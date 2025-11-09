using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Testes de integração para a API do módulo Users.
/// Valida endpoints, autenticação, autorização e respostas da API.
/// </summary>
/// <remarks>
/// Verifica se as funcionalidades principais estão funcionando:
/// - Endpoints estão acessíveis
/// - Respostas estão no formato correto
/// - Autorização está funcionando
/// - Dados são persistidos corretamente
/// </remarks>
public class UsersApiTests : InstanceApiTestBase
{
    [Fact]
    public async Task UsersEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task UsersEndpoint_WithAuthentication_ShouldReturnValidResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Admin users should receive a successful response");

        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement>(content);

        // Expect a consistent API response format - should be an object with data property
        users.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");
        users.TryGetProperty("data", out var dataElement).Should().BeTrue(
            "Response should contain 'data' property for consistency");
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
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var userData = new
        {
            username = $"test{Guid.NewGuid():N}"[..20], // Limit to 20 chars
            email = $"test-{Guid.NewGuid():N}@example.com",
            firstName = "Test",
            lastName = "User",
            keycloakId = $"keycloak-{Guid.NewGuid()}"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users", userData);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"POST requests that create resources should return 201 Created. Response: {content}");

        var responseJson = JsonSerializer.Deserialize<JsonElement>(content);

        // Verifica se é uma response estruturada (com data)
        var dataElement = GetResponseData(responseJson);
        dataElement.TryGetProperty("id", out _).Should().BeTrue(
            $"Response data should contain 'id' property. Full response: {content}");
        dataElement.TryGetProperty("username", out var usernameProperty).Should().BeTrue();
        usernameProperty.GetString().Should().Be(userData.username);

        // Cleanup - attempt to delete created user
        if (dataElement.TryGetProperty("id", out var idProperty))
        {
            var userId = idProperty.GetString();
            var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");
            // Note: We don't assert success here as cleanup is best-effort
        }
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
                // Log endpoint failure for debugging
                Console.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    private static JsonElement GetResponseData(JsonElement response)
    {
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }
}
