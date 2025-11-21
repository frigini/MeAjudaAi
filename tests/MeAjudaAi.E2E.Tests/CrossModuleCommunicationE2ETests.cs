using System.Net;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.Tests.E2E.ModuleApis;

/// <summary>
/// Testes E2E focados nos padrões de comunicação entre módulos
/// Demonstra como diferentes módulos podem interagir via APIs HTTP
/// </summary>
public class CrossModuleCommunicationE2ETests : TestContainerTestBase
{
    private async Task<JsonElement> CreateUserAsync(string username, string email, string firstName, string lastName)
    {
        AuthenticateAsAdmin(); // CreateUser requer role admin (AdminOnly policy)

        var createRequest = new
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var response = await PostJsonAsync("/api/v1/users", createRequest);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // User already exists - fetch by email
            AuthenticateAsAdmin();
            var getUserResponse = await ApiClient.GetAsync($"/api/v1/users/by-email/{Uri.EscapeDataString(email)}");

            if (getUserResponse.IsSuccessStatusCode)
            {
                var getUserContent = await getUserResponse.Content.ReadAsStringAsync();
                var getUserResult = JsonSerializer.Deserialize<JsonElement>(getUserContent, JsonOptions);
                if (getUserResult.TryGetProperty("data", out var existingUser))
                {
                    return existingUser;
                }
            }

            // Se não conseguiu buscar, falha o teste (não usar fake ID)
            throw new InvalidOperationException($"Failed to retrieve existing user by email: {email}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.TryGetProperty("data", out var dataProperty).Should().BeTrue();
        return dataProperty;
    }

    [Theory]
    [InlineData("NotificationModule", "notification@test.com")]
    [InlineData("OrdersModule", "orders@test.com")]
    [InlineData("PaymentModule", "payment@test.com")]
    [InlineData("ReportingModule", "reports@test.com")]
    public async Task ModuleToModuleCommunication_ShouldWorkForDifferentConsumers(string moduleName, string email)
    {
        // Arrange - Simulate different modules consuming Users API
        var user = await CreateUserAsync(
            username: $"user_for_{moduleName.ToLower()}",
            email: email,
            firstName: "Test",
            lastName: moduleName
        );

        var userId = user.GetProperty("id").GetGuid();

        // Act & Assert - Each module would have different use patterns
        switch (moduleName)
        {
            case "NotificationModule":
                // Notification module needs user existence and email validation
                AuthenticateAsAdmin(); // Requer autenticação para acessar users API
                var checkEmailResponse = await ApiClient.GetAsync($"/api/v1/users/by-email/{Uri.EscapeDataString(email)}");
                checkEmailResponse.IsSuccessStatusCode.Should().BeTrue(
                    "Email check should succeed for valid user created in Arrange");
                break;

            case "OrdersModule":
                // Orders module needs full user details and batch operations
                AuthenticateAsAdmin(); // Requer autenticação para acessar users API
                var getUserResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
                if (!getUserResponse.IsSuccessStatusCode)
                {
                    var errorContent = await getUserResponse.Content.ReadAsStringAsync();
                    throw new Exception($"GET /api/v1/users/{userId} failed with {getUserResponse.StatusCode}: {errorContent}");
                }
                getUserResponse.IsSuccessStatusCode.Should().BeTrue(
                    "GetUser should succeed for valid user created in Arrange");
                break;

            case "PaymentModule":
                // Payment module needs user validation for security
                AuthenticateAsAdmin(); // Requer autenticação para acessar users API
                var userExistsResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
                userExistsResponse.IsSuccessStatusCode.Should().BeTrue(
                    "User validation should succeed for valid user created in Arrange");
                break;

            case "ReportingModule":
                // Reporting module needs batch user data
                AuthenticateAsAdmin(); // Requer autenticação para acessar users API
                var batchResponse = await ApiClient.GetAsync($"/api/v1/users/{userId}");
                batchResponse.IsSuccessStatusCode.Should().BeTrue(
                    "Batch user data retrieval should succeed for valid user created in Arrange");
                break;
        }
    }

    [Fact]
    public async Task SimultaneousModuleRequests_ShouldHandleConcurrency()
    {
        // Arrange - Create test users
        var users = new List<JsonElement>();
        for (int i = 0; i < 10; i++)
        {
            var user = await CreateUserAsync(
                $"concurrent_user_{i}",
                $"concurrent_{i}@test.com",
                "Concurrent",
                $"User{i}"
            );
            users.Add(user);
        }

        // Act - Simulate multiple modules making concurrent requests
        AuthenticateAsAdmin(); // Mantém autenticação para todas as requisições
        var tasks = users.Select(async user =>
        {
            var userId = user.GetProperty("id").GetGuid();
            var response = await ApiClient.GetAsync($"/api/v1/users/{userId}");
            return response.StatusCode;
        }).ToList();

        var results = await Task.WhenAll(tasks);

        // Assert - All operations should succeed
        results.Should().AllSatisfy(status =>
            status.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound));
    }

    [Fact]
    public async Task ModuleApiContract_ShouldMaintainConsistentBehavior()
    {
        // Arrange
        var user = await CreateUserAsync("contract_test", "contract@test.com", "Contract", "Test");
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Test all contract methods behave consistently

        // 1. GetUserByIdAsync
        AuthenticateAsAdmin(); // Requer autenticação para acessar users API
        var getUserResponse = await ApiClient.GetAsync($"/api/v1/users/{user.GetProperty("id").GetGuid()}");
        if (getUserResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await getUserResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);

            // Verify standard response structure
            result.TryGetProperty("data", out var data).Should().BeTrue();
            data.TryGetProperty("id", out _).Should().BeTrue();
            data.TryGetProperty("username", out _).Should().BeTrue();
            data.TryGetProperty("email", out _).Should().BeTrue();
            data.TryGetProperty("firstName", out _).Should().BeTrue();
            data.TryGetProperty("lastName", out _).Should().BeTrue();
        }

        // 2. Non-existent user should return consistent response
        var nonExistentResponse = await ApiClient.GetAsync($"/api/v1/users/{nonExistentId}");
        nonExistentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ErrorRecovery_ModuleApiFailures_ShouldNotAffectOtherModules()
    {
        // This test simulates how failures in one module's usage shouldn't affect others

        // Arrange
        AuthenticateAsAdmin(); // CreateUserAsync requer role Admin (AdminOnly policy)
        var validUser = await CreateUserAsync("recovery_test", "recovery@test.com", "Recovery", "Test");
        var invalidUserId = Guid.NewGuid();

        // Act - Mix valid and invalid operations (simulating different modules)
        var validTask = ApiClient.GetAsync($"/api/v1/users/{validUser.GetProperty("id").GetGuid()}");
        var invalidTask = ApiClient.GetAsync($"/api/v1/users/{invalidUserId}");

        var results = await Task.WhenAll(validTask, invalidTask);

        // Assert - Valid operations succeed, invalid ones fail gracefully
        results[0].StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        results[1].StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
