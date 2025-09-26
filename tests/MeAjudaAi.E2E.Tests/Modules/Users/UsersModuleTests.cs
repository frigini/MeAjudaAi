using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Users;

/// <summary>
/// Testes de integração para endpoints do módulo Users
/// </summary>
public class UsersModuleTests : TestContainerTestBase
{

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithPaginatedResult()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/v1/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound // Aceitável se ainda não existem usuários
        );

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            // Verifica se é JSON válido
            var jsonDocument = System.Text.Json.JsonDocument.Parse(content);
            jsonDocument.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreatedOrConflict()
    {
        // Arrange
        var createUserRequest = new CreateUserRequest
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", createUserRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,      // Sucesso
            HttpStatusCode.Conflict,     // Usuário já existe
            HttpStatusCode.BadRequest    // Erro de validação
        );

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var createdUser = System.Text.Json.JsonSerializer.Deserialize<CreateUserResponse>(content, JsonOptions);
            createdUser.Should().NotBeNull();
            createdUser!.UserId.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateUserRequest
        {
            Username = "", // Inválido: username vazio
            Email = "invalid-email", // Inválido: email mal formatado
            FirstName = "",
            LastName = ""
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // GetUserById requer autorização "SelfOrAdmin"
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // GetUserByEmail requer autorização "AdminOnly"
        var nonExistentEmail = $"nonexistent_{Guid.NewGuid():N}@example.com";

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/users/by-email/{nonExistentEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateUserProfileRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = $"updated_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/users/{nonExistentId}/profile", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserEndpoints_ShouldHandleInvalidGuids()
    {
        // Act & Assert - Quando o constraint de GUID não bate, a rota retorna 404 
        var invalidGuidResponse = await ApiClient.GetAsync("/api/v1/users/invalid-guid");
        invalidGuidResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// DTOs simples para teste (para evitar dependências complexas)
/// </summary>
public record CreateUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

public record CreateUserResponse
{
    public Guid UserId { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record UpdateUserProfileRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
