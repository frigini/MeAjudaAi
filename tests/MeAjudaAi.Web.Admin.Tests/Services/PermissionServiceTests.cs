using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using MeAjudaAi.Web.Admin.Authorization;
using MeAjudaAi.Web.Admin.Services;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Web.Admin.Tests.Services;

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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
        var user = CreateAuthenticatedUser(role.ToUpper());
        var authState = new AuthenticationState(user);
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _permissionService.HasAnyRoleAsync(role.ToLower());

        // Assert
        Assert.True(result);
    }

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
