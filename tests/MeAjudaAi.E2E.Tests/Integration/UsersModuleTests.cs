using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração E2E para endpoints do módulo Users
/// Foca em cenários end-to-end complexos não cobertos por Integration tests
/// </summary>
/// <remarks>
/// NOTE: Testes simples como GetUserById_WithNonExistentId, GetUserByEmail_WithNonExistentEmail,
/// CreateUser_WithValidData e GetUsers básico foram removidos pois duplicam UsersIntegrationTests.cs
/// E2E tests devem focar em workflows complexos e cenários de integração entre módulos.
/// </remarks>
public class UsersModuleTests : TestContainerTestBase
{
    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        AuthenticateAsAdmin(); // CreateUser requer role admin (AdminOnly policy)
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
    public async Task UpdateUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // UpdateUserProfile requer autorização (SelfOrAdmin policy)
        var nonExistentId = UuidGenerator.NewId();
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
        AuthenticateAsAdmin(); // DELETE requer autorização Admin
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserEndpoints_ShouldHandleInvalidGuids()
    {
        // Arrange
        AuthenticateAsAdmin(); // GET requer autorização

        // Act & Assert - Quando o constraint de GUID não bate, a rota retorna 404 
        var invalidGuidResponse = await ApiClient.GetAsync("/api/v1/users/invalid-guid");
        invalidGuidResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Modelo de requisição para criar um novo usuário em testes E2E.
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// Obtém ou inicializa o nome de usuário.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o endereço de email.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o primeiro nome.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o último nome.
    /// </summary>
    public string LastName { get; init; } = string.Empty;
}

/// <summary>
/// Modelo de resposta para criação de usuário em testes E2E.
/// </summary>
public record CreateUserResponse
{
    /// <summary>
    /// Obtém ou inicializa o ID do usuário criado.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Obtém ou inicializa a mensagem de resposta.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Modelo de requisição para atualizar perfil de usuário em testes E2E.
/// </summary>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// Obtém ou inicializa o primeiro nome.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o último nome.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Obtém ou inicializa o endereço de email.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
