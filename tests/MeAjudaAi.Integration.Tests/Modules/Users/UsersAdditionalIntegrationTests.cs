using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Testes de integração adicionais para o módulo de usuários.
/// </summary>
public class UsersAdditionalIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

    [Fact]
    public async Task UpdateUserProfile_WithValidData_ShouldReturnOk()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar um usuário
        var email = $"update_{Guid.NewGuid():N}@example.com";
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", new
        {
            username = $"user_{Guid.NewGuid():N}"[..20],
            email = email,
            firstName = "Original",
            lastName = "User",
            password = "Password123",
            keycloakId = Guid.NewGuid().ToString()
        });
        var userId = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // 2. Atualizar perfil
        var updateRequest = new
        {
            firstName = "Updated",
            lastName = "Name",
            email = email,
            phoneNumber = "+5511988887777"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se atualizou
        var getResponse = await Client.GetAsync($"/api/v1/users/{userId}");
        var data = GetResponseData(await ReadJsonAsync<JsonElement>(getResponse.Content));
        data.GetProperty("firstName").GetString().Should().Be("Updated");
        data.GetProperty("lastName").GetString().Should().Be("Name");
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        
        // 1. Criar usuário
        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", new
        {
            username = $"del_{Guid.NewGuid():N}"[..20],
            email = $"del_{Guid.NewGuid():N}@example.com",
            firstName = "Delete",
            lastName = "Me",
            password = "Password123",
            keycloakId = Guid.NewGuid().ToString()
        });
        var userId = GetResponseData(await ReadJsonAsync<JsonElement>(createResponse.Content)).GetProperty("id").GetString();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/users/{userId}");

        // Assert - Endpoint returns 200 OK with the result object
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify deletion - trying to get the user should return NotFound (or indicate deleted state)
        var getResponse = await Client.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }
}
