using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Testes de integração para a API do módulo Users.
/// Valida endpoints, autenticação, autorização e respostas da API.
/// </summary>
/// <remarks>
/// Verifica se as funcionalidades principais estão funcionando:
/// - Endpoints estão acessíveis
/// - Respostas estão no formato correto
/// - Autorização está funcionando
/// - Dados são persistidos corretamente
/// </remarks>
public class UsersApiTests : ApiTestBase
{
    [Fact]
    public async Task UsersEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.OK);
    }

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

    [Fact]
    public async Task UsersEndpoints_AdminUser_ShouldNotReturnAuthorizationOrServerErrors()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/users"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                // Log endpoint failure for debugging
                Console.WriteLine($"Endpoint {endpoint} returned {response.StatusCode}. Body: {body}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Authenticated admin requests to {endpoint} should succeed.");
        }
    }

    private static JsonElement GetResponseData(JsonElement response)
    {
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }
}
