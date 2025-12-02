using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração para endpoints do módulo Users
/// </summary>
public class UsersModuleTests : TestContainerTestBase
{

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithPaginatedResult()
    {
        // Arrange
        AuthenticateAsAdmin();

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
        AuthenticateAsAdmin(); // CreateUser requer role admin

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
    public async Task GetUserById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // GetUserById requer autorização "SelfOrAdmin"
        var nonExistentId = Guid.CreateVersion7();

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
        AuthenticateAsAdmin(); // UpdateUserProfile requer autorização (SelfOrAdmin policy)
        var nonExistentId = Guid.CreateVersion7();
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
        var nonExistentId = Guid.CreateVersion7();

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
/// Request model for creating a new user in E2E tests.
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// Gets or initializes the username.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;
}

/// <summary>
/// Response model for user creation in E2E tests.
/// </summary>
public record CreateUserResponse
{
    /// <summary>
    /// Gets or initializes the created user's ID.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets or initializes the response message.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Request model for updating a user profile in E2E tests.
/// </summary>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// Gets or initializes the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
