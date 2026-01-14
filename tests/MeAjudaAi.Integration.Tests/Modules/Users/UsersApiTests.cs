using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Testes de integração para a API do módulo Users.
/// Valida formato de resposta e estrutura da API.
/// </summary>
/// <remarks>
/// Foca em validações de formato de resposta que não são cobertas por testes de negócio.
/// Testes de endpoints, autenticação e CRUD são cobertos por UsersIntegrationTests.cs
/// </remarks>
public class UsersApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

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
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var invalidRequest = new
        {
            Username = "", // Inválido: username vazio
            Email = "invalid-email", // Inválido: email mal formatado
            Password = "123", // Inválido: senha muito curta
            Role = "InvalidRole", // Inválido: role não existe
            FirstName = "",
            LastName = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid user data should return 400 Bad Request");
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "User",
            Email = $"updated_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/users/{nonExistentId}/profile", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Updating non-existent user should return 404 Not Found");
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Deleting non-existent user should return 404 Not Found");
    }

    [Fact]
    public async Task UserEndpoints_ShouldHandleInvalidGuids()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users/invalid-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Invalid GUID format should result in route not matching, returning 404");
    }
}
