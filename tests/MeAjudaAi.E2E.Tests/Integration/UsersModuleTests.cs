using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Users.Application.DTOs;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes de integração para endpoints do módulo Users
/// </summary>
public class UsersModuleTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithPaginatedResult()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/v1/users?page=1&pageSize=10");

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
            var jsonDocument = JsonDocument.Parse(content);
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
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users", createUserRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,      // Success
            HttpStatusCode.Conflict,     // User already exists
            HttpStatusCode.BadRequest    // Validation error
        );

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            var createdUser = JsonSerializer.Deserialize<CreateUserResponse>(content, _jsonOptions);
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
            Username = "", // Invalid: empty username
            Email = "invalid-email", // Invalid: malformed email
            FirstName = "",
            LastName = ""
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/v1/users", invalidRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentEmail = $"nonexistent_{Guid.NewGuid():N}@example.com";

        // Act
        var response = await HttpClient.GetAsync($"/api/v1/users/by-email/{nonExistentEmail}");

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
        var response = await HttpClient.PutAsJsonAsync($"/api/v1/users/{nonExistentId}", updateRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserEndpoints_ShouldHandleInvalidGuids()
    {
        // Act & Assert
        var invalidGuidResponse = await HttpClient.GetAsync("/api/v1/users/invalid-guid");
        invalidGuidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

/// <summary>
/// Simple DTOs for testing (to avoid complex dependencies)
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