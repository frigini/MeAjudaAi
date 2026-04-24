using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para AuthorizationExtensions
/// Cobertura: AddPermissionBasedAuthorization, AddKeycloakPermissionResolver, UsePermissionBasedAuthorization, etc.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Authorization")]
public class AuthorizationExtensionsTests
{
    [Fact]
    public void HasPermission_WithUserHavingPermission_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.UsersRead);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithUserNotHavingPermission_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.UsersCreate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WithNullPrincipal_ShouldThrowArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var action = () => principal!.HasPermission(EPermission.UsersRead);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasPermissions_WithUserHavingAllPermissions_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermissions_WithUserHavingOnePermission_RequireAll_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPermissions_WithUserHavingOnePermission_RequireAny_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermissions_WithUserHavingNoPermissions_RequireAny_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersCreate, EPermission.UsersDelete], requireAll: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPermissions_WithMultiplePermissions_ShouldReturnAllPermissions()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create"),
            new(AuthConstants.Claims.Permission, "users:update")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.UsersCreate);
        result.Should().Contain(EPermission.UsersUpdate);
    }

    [Fact]
    public void GetPermissions_WithNoPermissions_ShouldReturnEmptyList()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPermissions_ShouldExcludeProcessingMarker()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "*") // Processing marker
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(EPermission.UsersRead);
    }

    [Fact]
    public void IsSystemAdmin_WithSystemAdminClaim_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.IsSystemAdmin, "true")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSystemAdmin_WithoutSystemAdminClaim_ShouldReturnFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        result.Should().BeFalse();
    }

    // Test helper class
    private class TestModulePermissionResolver : IModulePermissionResolver
    {
        public string ModuleName => "Test";

        public Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EPermission> permissions = [EPermission.UsersRead];
            return Task.FromResult(permissions);
        }

        public bool CanResolve(EPermission permission)
        {
            return permission == EPermission.UsersRead || permission == EPermission.UsersCreate;
        }
    }
}

