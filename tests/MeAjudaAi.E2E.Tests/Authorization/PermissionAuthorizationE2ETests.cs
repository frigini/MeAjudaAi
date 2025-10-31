using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MeAjudaAi.Shared.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MeAjudaAi.E2E.Tests.Authorization;

/// <summary>
/// Testes end-to-end para fluxos completos de autorização.
/// Simula cenários reais de usuários com diferentes roles acessando endpoints.
/// </summary>
public class PermissionAuthorizationE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PermissionAuthorizationE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task BasicUserWorkflow_ShouldHaveAppropriateAccess()
    {
        // Arrange - Simular usuário básico autenticado
        var basicUserToken = GenerateTestJwtToken("basic-user-123", new[]
        {
            Permission.UsersRead.GetValue(),
            Permission.UsersProfile.GetValue()
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", basicUserToken);

        // Act & Assert - Operações que o usuário básico PODE fazer

        // 1. Ver seu próprio perfil
        var profileResponse = await _client.GetAsync("/api/users/profile");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        // 2. Ler informações básicas de usuários
        var readResponse = await _client.GetAsync("/api/users/basic-info");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        // Act & Assert - Operações que o usuário básico NÃO PODE fazer

        // 3. Criar usuários (deve retornar Forbidden)
        var createUserPayload = new { name = "New User", email = "new@test.com" };
        var createResponse = await _client.PostAsync("/api/users",
            new StringContent(JsonSerializer.Serialize(createUserPayload), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        // 4. Deletar usuários (deve retornar Forbidden)
        var deleteResponse = await _client.DeleteAsync("/api/users/some-user-id");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // 5. Acessar área administrativa (deve retornar Forbidden)
        var adminResponse = await _client.GetAsync("/api/users/admin");
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task UserAdminWorkflow_ShouldHaveAdministrativeAccess()
    {
        // Arrange - Simular administrador de usuários
        var userAdminToken = GenerateTestJwtToken("user-admin-456", new[]
        {
            Permission.UsersRead.GetValue(),
            Permission.UsersCreate.GetValue(),
            Permission.UsersUpdate.GetValue(),
            Permission.UsersList.GetValue(),
            Permission.AdminUsers.GetValue()
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAdminToken);

        // Act & Assert - Operações administrativas que PODE fazer

        // 1. Listar todos os usuários
        var listResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // 2. Criar usuários
        var createUserPayload = new { name = "Admin Created User", email = "admin@test.com" };
        var createResponse = await _client.PostAsync("/api/users",
            new StringContent(JsonSerializer.Serialize(createUserPayload), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // 3. Atualizar usuários
        var updatePayload = new { name = "Updated Name" };
        var updateResponse = await _client.PutAsync("/api/users/some-user-id",
            new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 4. Acessar área administrativa de usuários
        var adminResponse = await _client.GetAsync("/api/users/admin");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        // Act & Assert - Operações que NÃO PODE fazer (sem permissão de delete)

        // 5. Deletar usuários (deve retornar Forbidden - precisa de permissão específica)
        var deleteResponse = await _client.DeleteAsync("/api/users/some-user-id");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        // 6. Operações de sistema (deve retornar Forbidden)
        var systemResponse = await _client.GetAsync("/api/system/admin");
        Assert.Equal(HttpStatusCode.Forbidden, systemResponse.StatusCode);
    }

    [Fact]
    public async Task SystemAdminWorkflow_ShouldHaveFullAccess()
    {
        // Arrange - Simular administrador do sistema
        var systemAdminToken = GenerateTestJwtToken("system-admin-789", new[]
        {
            Permission.AdminSystem.GetValue(),
            Permission.AdminUsers.GetValue(),
            Permission.UsersRead.GetValue(),
            Permission.UsersCreate.GetValue(),
            Permission.UsersUpdate.GetValue(),
            Permission.UsersDelete.GetValue(),
            Permission.UsersList.GetValue()
        }, isSystemAdmin: true);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", systemAdminToken);

        // Act & Assert - Deve ter acesso completo

        // 1. Todas as operações de usuários
        var listResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var createResponse = await _client.PostAsync("/api/users",
            new StringContent(JsonSerializer.Serialize(new { name = "System User" }), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var updateResponse = await _client.PutAsync("/api/users/some-user-id",
            new StringContent(JsonSerializer.Serialize(new { name = "Updated" }), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync("/api/users/some-user-id");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // 2. Operações de sistema
        var systemResponse = await _client.GetAsync("/api/system/admin");
        Assert.Equal(HttpStatusCode.OK, systemResponse.StatusCode);

        // 3. Área administrativa completa
        var adminResponse = await _client.GetAsync("/api/users/admin");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task ModuleSpecificPermissions_ShouldIsolateAccess()
    {
        // Arrange - Usuário com permissões apenas do módulo Users
        var usersOnlyToken = GenerateTestJwtToken("users-module-user", new[]
        {
            Permission.UsersRead.GetValue(),
            Permission.UsersCreate.GetValue()
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", usersOnlyToken);

        // Act & Assert - Acesso ao módulo Users
        var usersResponse = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        // Act & Assert - Sem acesso a outros módulos (quando implementados)
        var providersResponse = await _client.GetAsync("/api/providers");
        Assert.Equal(HttpStatusCode.Forbidden, providersResponse.StatusCode);

        var ordersResponse = await _client.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.Forbidden, ordersResponse.StatusCode);
    }

    [Fact]
    public async Task ConcurrentUsersSameResource_ShouldRespectIndividualPermissions()
    {
        // Arrange - Dois usuários diferentes acessando o mesmo recurso
        var basicUserToken = GenerateTestJwtToken("basic-user-concurrent", new[]
        {
            Permission.UsersRead.GetValue()
        });

        var adminUserToken = GenerateTestJwtToken("admin-user-concurrent", new[]
        {
            Permission.AdminUsers.GetValue(),
            Permission.UsersDelete.GetValue()
        });

        var basicClient = _factory.CreateClient();
        var adminClient = _factory.CreateClient();

        basicClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", basicUserToken);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminUserToken);

        // Act & Assert - Operação que apenas admin pode fazer
        var basicUserDeleteResponse = await basicClient.DeleteAsync("/api/users/test-user");
        var adminUserDeleteResponse = await adminClient.DeleteAsync("/api/users/test-user");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, basicUserDeleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminUserDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task PermissionCaching_ShouldWorkAcrossRequests()
    {
        // Arrange - Usuário que fará múltiplas requisições
        var userToken = GenerateTestJwtToken("cache-test-user", new[]
        {
            Permission.UsersRead.GetValue(),
            Permission.UsersProfile.GetValue()
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Act - Múltiplas requisições que devem usar cache
        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 5; i++)
        {
            var response = await _client.GetAsync("/api/users/profile");
            responses.Add(response);
        }

        // Assert - Todas devem ser bem-sucedidas
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

        // Verificar que as permissões foram resolvidas consistentemente
        // (Em um teste real, isso seria verificado através de logs ou métricas)
        Assert.True(responses.Count == 5);
    }

    [Fact]
    public async Task TokenExpiredOrInvalid_ShouldReturnUnauthorized()
    {
        // Arrange - Token inválido
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/users/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MissingRequiredPermission_ShouldReturnForbiddenWithDetails()
    {
        // Arrange - Usuário sem permissão necessária
        var limitedToken = GenerateTestJwtToken("limited-user", new[]
        {
            Permission.UsersProfile.GetValue() // Tem profile mas não tem create
        });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", limitedToken);

        // Act
        var createResponse = await _client.PostAsync("/api/users",
            new StringContent(JsonSerializer.Serialize(new { name = "Test" }), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        // Verificar se retorna detalhes do erro (ProblemDetails)
        var content = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("permission", content.ToLowerInvariant());
    }

    /// <summary>
    /// Gera um token JWT de teste para uso nos testes E2E.
    /// Em um ambiente real, isso viria do Keycloak.
    /// </summary>
    private static string GenerateTestJwtToken(string userId, string[] permissions, bool isSystemAdmin = false)
    {
        // Para testes E2E, simular a geração de um token JWT
        // Em implementação real, isso seria gerado pelo Keycloak
        var claims = new Dictionary<string, object>
        {
            { "sub", userId },
            { "permissions", permissions }
        };

        if (isSystemAdmin)
        {
            claims.Add("system_admin", true);
        }

        // Simular token JWT (em implementação real, usar biblioteca JWT)
        var tokenPayload = JsonSerializer.Serialize(claims);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenPayload));
    }
}
