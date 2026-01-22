using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using MeAjudaAi.Web.Admin.Authorization;
using MeAjudaAi.Web.Admin.Services;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Web.Admin.Tests.Services;

/// <summary>
/// Testes unitários para o serviço de permissões.
/// Valida autorização, verificação de roles e políticas de segurança.
/// </summary>
public class PermissionServiceTests
{
    private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
    private readonly Mock<IAuthorizationService> _authServiceMock;
    private readonly Mock<ILogger<PermissionService>> _loggerMock;
    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();
        _authServiceMock = new Mock<IAuthorizationService>();
        _loggerMock = new Mock<ILogger<PermissionService>>();

        _permissionService = new PermissionService(
            _authStateProviderMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_WithValidPolicy_ReturnsTrue()
    {
        // Arrange - Usuário autenticado com política válida
        var user = CreateAuthenticatedUser(RoleNames.Admin);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        _authServiceMock.Setup(x => x.AuthorizeAsync(user, null, PolicyNames.AdminPolicy))
            .ReturnsAsync(AuthorizationResult.Success());

        // Act
        var result = await _permissionService.HasPermissionAsync(PolicyNames.AdminPolicy);

        // Assert
        Assert.True(result);
        _authServiceMock.Verify(x => x.AuthorizeAsync(user, null, PolicyNames.AdminPolicy), Times.Once);
    }

    [Fact]
    public async Task HasPermissionAsync_WithInvalidPolicy_ReturnsFalse()
    {
        // Arrange - Usuário com role insuficiente para a política
        var user = CreateAuthenticatedUser(RoleNames.Viewer);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        _authServiceMock.Setup(x => x.AuthorizeAsync(user, null, PolicyNames.AdminPolicy))
            .ReturnsAsync(AuthorizationResult.Failed());

        // Act
        var result = await _permissionService.HasPermissionAsync(PolicyNames.AdminPolicy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WithUnauthenticatedUser_ReturnsFalse()
    {
        // Arrange - Usuário não autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasPermissionAsync(PolicyNames.AdminPolicy);

        // Assert
        Assert.False(result);
        _authServiceMock.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HasAnyRoleAsync_WithMatchingRole_ReturnsTrue()
    {
        // Arrange - Usuário com role esperada
        var user = CreateAuthenticatedUser(RoleNames.Admin, RoleNames.ProviderManager);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAnyRoleAsync(RoleNames.Admin);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAnyRoleAsync_WithMultipleRolesOneMatches_ReturnsTrue()
    {
        // Arrange - Usuário com uma das roles esperadas
        var user = CreateAuthenticatedUser(RoleNames.ProviderManager);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAnyRoleAsync(RoleNames.Admin, RoleNames.ProviderManager);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAnyRoleAsync_WithNoMatchingRole_ReturnsFalse()
    {
        // Arrange - Usuário sem nenhuma das roles esperadas
        var user = CreateAuthenticatedUser(RoleNames.Viewer);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAnyRoleAsync(RoleNames.Admin, RoleNames.ProviderManager);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasAllRolesAsync_WithAllRoles_ReturnsTrue()
    {
        // Arrange - Usuário com todas as roles esperadas
        var user = CreateAuthenticatedUser(RoleNames.Admin, RoleNames.ProviderManager);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAllRolesAsync(RoleNames.Admin, RoleNames.ProviderManager);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAllRolesAsync_WithMissingRole_ReturnsFalse()
    {
        // Arrange - Usuário com apenas uma das roles esperadas
        var user = CreateAuthenticatedUser(RoleNames.Admin);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAllRolesAsync(RoleNames.Admin, RoleNames.ProviderManager);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserRolesAsync_ReturnsAllUserRoles()
    {
        // Arrange - Usuário com múltiplas roles
        var user = CreateAuthenticatedUser(RoleNames.Admin, RoleNames.ProviderManager);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var roles = await _permissionService.GetUserRolesAsync();

        // Assert
        Assert.Contains(RoleNames.Admin, roles);
        Assert.Contains(RoleNames.ProviderManager, roles);
        Assert.Equal(2, roles.Count());
    }

    [Fact]
    public async Task GetUserRolesAsync_WithUnauthenticatedUser_ReturnsEmpty()
    {
        // Arrange - Usuário não autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var roles = await _permissionService.GetUserRolesAsync();

        // Assert
        Assert.Empty(roles);
    }

    [Fact]
    public async Task IsAdminAsync_WithAdminRole_ReturnsTrue()
    {
        // Arrange - Usuário com role de administrador
        var user = CreateAuthenticatedUser(RoleNames.Admin);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.IsAdminAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAdminAsync_WithoutAdminRole_ReturnsFalse()
    {
        // Arrange - Usuário sem role de administrador
        var user = CreateAuthenticatedUser(RoleNames.Viewer);
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.IsAdminAsync();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(RoleNames.Admin)]
    [InlineData(RoleNames.ProviderManager)]
    [InlineData(RoleNames.DocumentReviewer)]
    public async Task HasAnyRoleAsync_IsCaseInsensitive(string role)
    {
        // Arrange - Verificação case-insensitive de roles
        var user = CreateAuthenticatedUser(role.ToUpper());
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAnyRoleAsync(role.ToLower());

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Cria um usuário autenticado com as roles especificadas para uso nos testes.
    /// Usa formato Keycloak (claim "roles") para simular autenticação OIDC.
    /// </summary>
    private static ClaimsPrincipal CreateAuthenticatedUser(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.NameIdentifier, "123")
        };

        // Add roles using the "roles" claim (Keycloak format)
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
