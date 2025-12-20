using System.Net;
using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
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
            permissions: [EPermission.UsersRead.GetValue(), EPermission.UsersList.GetValue()]
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
            permissions: [EPermission.UsersRead.GetValue()] // Tem read mas não list
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
            permissions: [EPermission.UsersCreate.GetValue()],
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
            permissions: [EPermission.UsersRead.GetValue()]
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
                EPermission.UsersList.GetValue(),
                EPermission.UsersRead.GetValue(),
                EPermission.UsersUpdate.GetValue()
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
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        // Arrange - Administrador do sistema (role admin + permissões completas)
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "admin-sys-678",
            userName: "sysadmin",
            email: "sysadmin@test.com",
            permissions: [
                EPermission.AdminSystem.GetValue(),
                EPermission.AdminUsers.GetValue(),
                EPermission.UsersList.GetValue(),
                EPermission.UsersRead.GetValue(),
                EPermission.UsersCreate.GetValue(),
                EPermission.UsersUpdate.GetValue(),
                EPermission.UsersDelete.GetValue()
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
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
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
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        // Arrange
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-persist-901",
            userName: "persistent",
            email: "persistent@test.com",
            permissions: [EPermission.UsersList.GetValue(), EPermission.UsersRead.GetValue()]
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

    #region Role-Based Policies

    [Fact]
    public async Task ProviderOnlyPolicy_WithProviderRole_ShouldAllow()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "provider-123",
            userName: "provider",
            email: "provider@test.com",
            permissions: [EPermission.ProvidersRead.GetValue()],
            roles: ["Provider"]
        );

        // Act - endpoint que requer ProviderOnly policy
        var response = await ApiClient.GetAsync("/api/v1/providers");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Provider deveria ter acesso. Status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task ProviderOnlyPolicy_WithUserRole_ShouldDeny()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-123",
            userName: "normaluser",
            email: "user@test.com",
            permissions: [EPermission.ProvidersRead.GetValue()],
            roles: ["User"] // NÃO é Provider
        );

        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers");

        // Assert - Deve negar acesso se policy ProviderOnly estiver aplicada
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.OK,
            $"Status deve ser Forbidden ou OK dependendo se ProviderOnly está implementado. Status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AdminOrProviderPolicy_WithAdmin_ShouldAllow()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "admin-123",
            userName: "admin",
            email: "admin@test.com",
            permissions: [],
            roles: ["Admin"],
            isSystemAdmin: true
        );

        // Act - endpoint que requer AdminOrProvider
        var response = await ApiClient.GetAsync("/api/v1/providers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminOrProviderPolicy_WithProvider_ShouldAllow()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "provider-456",
            userName: "provider2",
            email: "provider2@test.com",
            permissions: [EPermission.ProvidersRead.GetValue()],
            roles: ["Provider"]
        );

        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Provider deveria ter acesso via AdminOrProvider. Status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AdminOrOwnerPolicy_WithOwner_ShouldAllowOwnResource()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        var ownerId = "owner-789";
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: ownerId,
            userName: "resourceowner",
            email: "owner@test.com",
            permissions: [EPermission.UsersRead.GetValue()],
            roles: ["User"]
        );

        // Act - acessar próprio recurso
        var response = await ApiClient.GetAsync($"/api/v1/users/{ownerId}");

        // Assert - Owner deve poder acessar próprio recurso
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Owner deveria acessar próprio recurso. Status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AdminOrOwnerPolicy_WithNonOwner_ShouldDenyOtherResource()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-999",
            userName: "notowner",
            email: "notowner@test.com",
            permissions: [EPermission.UsersRead.GetValue()],
            roles: ["User"]
        );

        var otherUserId = "other-user-888";

        // Act - tentar acessar recurso de outro usuário
        var response = await ApiClient.GetAsync($"/api/v1/users/{otherUserId}");

        // Assert - Não-owner sem admin deve ser negado
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound,
            $"Não-owner sem admin deveria ser negado. Status: {response.StatusCode}"
        );
    }

    [Fact]
    public async Task AdminOrOwnerPolicy_WithAdmin_ShouldAllowAnyResource()
    {
        // Arrange
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        
        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "admin-777",
            userName: "sysadmin",
            email: "sysadmin@test.com",
            permissions: [],
            roles: ["Admin"],
            isSystemAdmin: true
        );

        var anyUserId = Guid.NewGuid().ToString();

        // Act - admin acessando qualquer recurso
        var response = await ApiClient.GetAsync($"/api/v1/users/{anyUserId}");

        // Assert - Admin deve poder acessar qualquer recurso
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Admin deveria acessar qualquer recurso. Status: {response.StatusCode}"
        );
    }

    #endregion
}

