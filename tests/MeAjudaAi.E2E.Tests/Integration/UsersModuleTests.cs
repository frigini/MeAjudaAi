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
    // NOTE: GetUsers_ShouldReturnOkWithPaginatedResult removed - duplicates UsersIntegrationTests.GetUsers_ShouldReturnUsersList
    // NOTE: CreateUser_WithValidData_ShouldReturnCreatedOrConflict removed - duplicates UsersIntegrationTests.CreateUser_WithValidData_ShouldReturnCreated

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

    // NOTE: GetUserById_WithNonExistentId_ShouldReturnNotFound removed - duplicates UsersIntegrationTests
    // NOTE: GetUserByEmail_WithNonExistentEmail_ShouldReturnNotFound removed - duplicates UsersIntegrationTests

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
