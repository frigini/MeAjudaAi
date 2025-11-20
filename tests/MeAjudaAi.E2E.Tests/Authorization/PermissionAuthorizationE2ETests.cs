using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.E2E.Tests.Authorization;

/// <summary>
/// Testes end-to-end para autorização baseada em permissions.
/// Valida que o ConfigurableTestAuthenticationHandler funciona corretamente com permissions customizadas.
/// </summary>
public class PermissionAuthorizationE2ETests : TestContainerTestBase
{
    [Fact]
    public async Task UserWithReadPermission_CanListUsers()
    {
        // Arrange - Limpar estado de testes anteriores
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Usuário com permissão de leitura
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-read-123",
            userName: "reader",
            email: "reader@test.com",
            permissions: [Permission.UsersRead.GetValue(), Permission.UsersList.GetValue()]
        );

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UserWithoutListPermission_CannotListUsers()
    {
        // Arrange - Limpar estado de testes anteriores
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Usuário sem permissão de listagem
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-noperm-456",
            userName: "noperm",
            email: "noperm@test.com",
            permissions: [Permission.UsersRead.GetValue()] // Tem read mas não list
        );

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert - Deve ser Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithCreatePermission_CanCreateUser()
    {
        // Arrange - Limpar estado de testes anteriores
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Usuário com permissão de criação E role admin (CreateUser requer AdminOnly policy)
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-creator-789",
            userName: "creator",
            email: "creator@test.com",
            permissions: [Permission.UsersCreate.GetValue()],
            isSystemAdmin: false,
            roles: ["admin"] // Necessário para passar pela policy AdminOnly
        );

        var newUser = new
        {
            Name = "Test User",
            Email = $"test-create-{Guid.NewGuid()}@example.com",
            Username = $"user-{Guid.NewGuid().ToString()[..8]}",
            Password = "Test@123456789" // Senha mais forte para passar validação
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", newUser);

        // Assert - Pode retornar BadRequest por validação, mas não deve ser Forbidden
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithoutCreatePermission_CannotCreateUser()
    {
        // Arrange - Limpar estado de testes anteriores
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Usuário sem permissão de criação
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-readonly-012",
            userName: "readonly",
            email: "readonly@test.com",
            permissions: [Permission.UsersRead.GetValue()]
        );

        var newUser = new
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Test@123!",
            Role = "user"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", newUser);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithMultiplePermissions_HasAppropriateAccess()
    {
        // Arrange - IMPORTANTE: Limpar estado de testes anteriores
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Usuário com múltiplas permissões (sem role admin, então não pode criar)
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-multi-345",
            userName: "multi",
            email: "multi@test.com",
            permissions: [
                Permission.UsersList.GetValue(),
                Permission.UsersRead.GetValue(),
                Permission.UsersUpdate.GetValue()
            ],
            isSystemAdmin: false,
            roles: [] // SEM role admin - testa que permissões funcionam mas policies de role também
        );

        // Act & Assert - Pode listar
        var listResponse = await ApiClient.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Testa criar usuário - DEVE retornar Forbidden pois não tem role admin
        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", new
        {
            Name = "Test",
            Email = $"test-{Guid.NewGuid()}@test.com",
            Username = $"user{Guid.NewGuid().ToString()[..8]}",
            Password = "Test@123456789"
        });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task SystemAdmin_HasFullAccess()
    {
        // Arrange - Administrador do sistema (role admin + permissões completas)
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "admin-sys-678",
            userName: "sysadmin",
            email: "sysadmin@test.com",
            permissions: [
                Permission.AdminSystem.GetValue(),
                Permission.AdminUsers.GetValue(),
                Permission.UsersList.GetValue(),
                Permission.UsersRead.GetValue(),
                Permission.UsersCreate.GetValue(),
                Permission.UsersUpdate.GetValue(),
                Permission.UsersDelete.GetValue()
            ],
            isSystemAdmin: true,
            roles: ["admin"] // Necessário para passar pela policy AdminOnly
        );

        // Act & Assert - Pode listar
        var listResponse = await ApiClient.GetAsync("/api/v1/users");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Pode criar (mesmo que dê BadRequest por validação, não deve ser Forbidden)
        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/users", new
        {
            Name = "Admin Created",
            Email = $"admin-{Guid.NewGuid()}@test.com",
            Username = $"admin{Guid.NewGuid().ToString()[..8]}",
            Password = "Admin@123456789"
        });
        Assert.NotEqual(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task UserWithoutAnyPermissions_ReturnsForbidden()
    {
        // Arrange - Usuário autenticado mas SEM nenhuma permissão
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-noperm-999",
            userName: "noperms",
            email: "noperms@test.com",
            permissions: [], // SEM permissões
            isSystemAdmin: false,
            roles: [] // SEM roles
        );

        // Act
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert - Não tem permissão UsersList, deve retornar Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserPermissionsWork_AcrossMultipleRequests()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-persist-901",
            userName: "persistent",
            email: "persistent@test.com",
            permissions: [Permission.UsersList.GetValue(), Permission.UsersRead.GetValue()]
        );

        // Act - Fazer múltiplas requisições
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 3; i++)
        {
            responses.Add(await ApiClient.GetAsync("/api/v1/users"));
        }

        // Assert - Todas devem ter sucesso
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
    }
}

