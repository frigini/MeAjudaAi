using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

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

        // Espera um formato de resposta API consistente - deve ser um objeto com propriedade data ou value
        users.ValueKind.Should().Be(JsonValueKind.Object,
            "API should return a structured response object");

        var dataElement = GetResponseData(users);
        (users.TryGetProperty("value", out _) || users.TryGetProperty("data", out _)).Should().BeTrue(
            "Response should contain 'data' or 'value' property for consistency");

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

    #region Public Endpoints

    [Fact]
    public async Task RegisterCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var registerRequest = new
        {
            Name = "New Customer",
            Email = $"customer_{Guid.NewGuid():N}@example.com",
            Password = "SecurePassword123!",
            PhoneNumber = "+5511999999999",
            TermsAccepted = true,
            AcceptedPrivacyPolicy = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "RegisterCustomer should return 201 Created for successful registration");
        var content = await ReadJsonAsync<JsonElement>(response.Content);
        GetResponseData(content).GetProperty("email").GetString().Should().Be(registerRequest.Email);
    }

    [Fact]
    public async Task GetAuthProviders_ShouldReturnStringArray()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/auth/providers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var providers = await response.Content.ReadFromJsonAsync<string[]>();
        providers.Should().NotBeNull();
        providers.Should().Contain("Google"); // Keycloak is disabled in test environment
    }

    #endregion

    #region User Retrieval Endpoints

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var email = $"getbyid_{Guid.NewGuid():N}@example.com";
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", new
        {
            username = $"user_{Guid.NewGuid():N}"[..20],
            email = email,
            firstName = "Get",
            lastName = "Byid",
            password = "Password123",
            keycloakId = Guid.NewGuid().ToString()
        });
        var userId = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(response.Content));
        data.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmail_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var email = $"getbyemail_{Guid.NewGuid():N}@example.com";
        await Client.PostAsJsonAsync("/api/v1/users", new
        {
            username = $"user_{Guid.NewGuid():N}"[..20],
            email = email,
            firstName = "Get",
            lastName = "ByEmail",
            password = "Password123",
            keycloakId = Guid.NewGuid().ToString()
        });

        // Act
        var response = await Client.GetAsync($"/api/v1/users/by-email/{email}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(response.Content));
        data.GetProperty("email").GetString().Should().Be(email);
    }

    #endregion

    #region Search and Contract Tests

    [Fact]
    public async Task CreateUser_ShouldReturnPhoneNumberInResponse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var phoneNumber = "+5511999999999";
        var createRequest = new
        {
            username = $"phonetest_{Guid.NewGuid():N}"[..20],
            email = $"phone_{Guid.NewGuid():N}@example.com",
            firstName = "Phone",
            lastName = "Test",
            password = "Password123!",
            keycloakId = Guid.NewGuid().ToString(),
            phoneNumber = phoneNumber
        };

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var data = GetResponseData(content);

        // Verify PhoneNumber is present in response
        data.TryGetProperty("phoneNumber", out var phoneNumberProperty).Should().BeTrue(
            "User response should contain phoneNumber field");
        phoneNumberProperty.GetString().Should().Be("11999999999",
            "PhoneNumber deve ser normalizado (sem prefixo +55) pelo domínio");

        // Cleanup
        var userId = data.GetProperty("id").GetGuid();
        await Client.DeleteAsync($"/api/v1/users/{userId}");
    }

    [Fact]
    public async Task CreateUser_ShouldReturnIsActiveField()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var createRequest = new
        {
            username = $"activetest_{Guid.NewGuid():N}"[..20],
            email = $"active_{Guid.NewGuid():N}@example.com",
            firstName = "Active",
            lastName = "Test",
            password = "Password123!",
            keycloakId = Guid.NewGuid().ToString()
        };

        // Act
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var data = GetResponseData(content);

        // Verify IsActive is present and true for new user
        data.TryGetProperty("isActive", out var isActiveProperty).Should().BeTrue(
            "User response should contain isActive field");
        isActiveProperty.GetBoolean().Should().BeTrue(
            "Newly created user should have isActive = true");

        // Cleanup
        var userId = data.GetProperty("id").GetGuid();
        await Client.DeleteAsync($"/api/v1/users/{userId}");
    }

    [Fact]
    public async Task DeleteUser_ShouldSetIsActiveToFalse()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var createRequest = new
        {
            username = $"deletetest_{Guid.NewGuid():N}"[..20],
            email = $"delete_{Guid.NewGuid():N}@example.com",
            firstName = "Delete",
            lastName = "Test",
            password = "Password123!",
            keycloakId = Guid.NewGuid().ToString()
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdContent = await ReadJsonAsync<JsonElement>(createResponse.Content);
        var userId = GetResponseData(createdContent).GetProperty("id").GetGuid();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Usuário deletado (soft delete) é filtrado pelo HasQueryFilter(u => !u.IsDeleted) no EF Core,
        // portanto GET retorna 404 NotFound para usuários soft-deleted.
        var getResponse = await Client.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Soft-deleted users are filtered by EF Core global query filter, so GET returns 404");
    }

    #endregion
}
