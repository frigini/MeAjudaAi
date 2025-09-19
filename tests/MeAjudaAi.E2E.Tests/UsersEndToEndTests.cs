using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.E2E.Tests;
using FluentAssertions;
using System.Net;

namespace MeAjudaAi.Integration.Tests.EndToEnd;

/// <summary>
/// Testes end-to-end para módulo Users
/// Testa fluxos completos de usuário através da API com infraestrutura real
/// </summary>
public class UsersEndToEndTests : EndToEndTestBase
{
    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreatedUser()
    {
        // Arrange
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        // Act
        var response = await PostJsonAsync("/api/v1/users", createUserRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdUser = await ReadJsonAsync<CreateUserResponse>(response);
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBeEmpty();
        createdUser.Username.Should().Be(createUserRequest.Username);
        createdUser.Email.Should().Be(createUserRequest.Email);
        createdUser.FirstName.Should().Be(createUserRequest.FirstName);
        createdUser.LastName.Should().Be(createUserRequest.LastName);
        createdUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = Faker.Internet.Email();
        
        var firstUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = email,
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };
        
        var duplicateUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = email, // Same email
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        // Act
        await PostJsonAsync("/api/v1/users", firstUserRequest);
        var duplicateResponse = await PostJsonAsync("/api/v1/users", duplicateUserRequest);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnUser()
    {
        // Arrange - Create a user first
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        var createResponse = await PostJsonAsync("/api/v1/users", createUserRequest);
        var createdUser = await ReadJsonAsync<CreateUserResponse>(createResponse);

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await ReadJsonAsync<GetUserResponse>(response);
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.Username.Should().Be(createUserRequest.Username);
        user.Email.Should().Be(createUserRequest.Email);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnUpdatedUser()
    {
        // Arrange - Create a user first
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        var createResponse = await PostJsonAsync("/api/v1/users", createUserRequest);
        var createdUser = await ReadJsonAsync<CreateUserResponse>(createResponse);

        var updateRequest = new
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName"
        };

        // Act
        var response = await PutJsonAsync($"/api/v1/users/{createdUser!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedUser = await ReadJsonAsync<UpdateUserResponse>(response);
        updatedUser.Should().NotBeNull();
        updatedUser!.Id.Should().Be(createdUser.Id);
        updatedUser.FirstName.Should().Be(updateRequest.FirstName);
        updatedUser.LastName.Should().Be(updateRequest.LastName);
        updatedUser.UpdatedAt.Should().BeAfter(updatedUser.CreatedAt);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Create a user first
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        var createResponse = await PostJsonAsync("/api/v1/users", createUserRequest);
        var createdUser = await ReadJsonAsync<CreateUserResponse>(createResponse);

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is deleted
        var getResponse = await ApiClient.GetAsync($"/api/v1/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange - Create multiple users
        var users = new List<CreateUserResponse>();
        for (int i = 0; i < 3; i++)
        {
            var createUserRequest = new
            {
                Username = $"testuser{i}_{Faker.Random.String(5)}",
                Email = Faker.Internet.Email(),
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                Password = "TempPassword123!"
            };

            var createResponse = await PostJsonAsync("/api/v1/users", createUserRequest);
            var createdUser = await ReadJsonAsync<CreateUserResponse>(createResponse);
            users.Add(createdUser!);
        }

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var pagedResult = await ReadJsonAsync<PaginatedResponse<GetUserResponse>>(response);
        pagedResult.Should().NotBeNull();
        pagedResult!.Items.Should().HaveCount(c => c <= 2);
        pagedResult.Page.Should().Be(1);
        pagedResult.PageSize.Should().Be(2);
        pagedResult.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task UserWorkflow_CompleteFlow_ShouldWorkEndToEnd()
    {
        // This test validates the complete user lifecycle
        
        // 1. Create user
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            Password = "TempPassword123!"
        };

        var createResponse = await PostJsonAsync("/api/v1/users", createUserRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await ReadJsonAsync<CreateUserResponse>(createResponse);

        // 2. Get user
        var getResponse = await ApiClient.GetAsync($"/api/v1/users/{createdUser!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Update user
        var updateRequest = new { FirstName = "Updated", LastName = "Name" };
        var updateResponse = await PutJsonAsync($"/api/v1/users/{createdUser.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verify update
        var verifyResponse = await ApiClient.GetAsync($"/api/v1/users/{createdUser.Id}");
        var updatedUser = await ReadJsonAsync<GetUserResponse>(verifyResponse);
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("Name");

        // 5. Delete user
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/users/{createdUser.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verify deletion
        var finalGetResponse = await ApiClient.GetAsync($"/api/v1/users/{createdUser.Id}");
        finalGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}