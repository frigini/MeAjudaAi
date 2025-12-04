using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Testes de integração para a API do módulo Users.
/// Valida formato de resposta e estrutura da API.
/// </summary>
/// <remarks>
/// Foca em validações de formato de resposta que não são cobertas por testes de negócio.
/// Testes de endpoints, autenticação e CRUD são cobertos por UsersIntegrationTests.cs
/// </remarks>
public class UsersApiTests : ApiTestBase
{
    // NOTE: UsersEndpoint_ShouldBeAccessible removed - low value smoke test
    // Endpoint existence is validated by all other tests

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

    // NOTE: GetUserById_WithNonExistentId, GetUserByEmail_WithNonExistentEmail, and CreateUser tests
    // are covered by UsersIntegrationTests.cs - removed duplicates to reduce test overhead
    
    // NOTE: UsersEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors removed
    // - Duplicates UsersIntegrationTests.UsersEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors
}
