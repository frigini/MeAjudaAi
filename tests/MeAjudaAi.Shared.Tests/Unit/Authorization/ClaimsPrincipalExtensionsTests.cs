using System.Security.Claims;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para as extensões de ClaimsPrincipal relacionadas a permissões.
/// </summary>
public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void HasPermission_WithValidPermission_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue()),
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersProfile.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.UsersRead);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_WithoutPermission_ShouldReturnFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.AdminSystem);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_WithUnauthenticatedUser_ShouldReturnFalse()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.HasPermission(EPermission.UsersRead);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermissions_WithAllRequiredPermissions_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue()),
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersCreate.GetValue()),
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersProfile.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions(new[] { EPermission.UsersRead, EPermission.UsersCreate }, requireAll: true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermissions_WithMissingPermission_ShouldReturnFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions(new[] { EPermission.UsersRead, EPermission.AdminSystem }, requireAll: true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAnyPermission_WithAtLeastOnePermission_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions(new[] { EPermission.UsersRead, EPermission.AdminSystem }, requireAll: false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAnyPermission_WithNoMatchingPermissions_ShouldReturnFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions(new[] { EPermission.AdminSystem, EPermission.AdminUsers }, requireAll: false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPermissions_ShouldReturnAllUserPermissions()
    {
        // Arrange
        var expectedPermissions = new[] { EPermission.UsersRead, EPermission.UsersProfile, EPermission.UsersList };
        var claims = expectedPermissions.Select(p => new Claim(AuthConstants.Claims.Permission, p.GetValue())).ToArray();
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        Assert.Equal(expectedPermissions.Length, result.Count);
        Assert.All(expectedPermissions, permission => Assert.Contains(permission, result));
    }

    [Fact]
    public void GetPermissions_WithUnauthenticatedUser_ShouldReturnEmpty()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetPermissions();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsSystemAdmin_WithSystemAdminClaim_ShouldReturnTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.IsSystemAdmin, "true")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSystemAdmin_WithoutSystemAdminClaim_ShouldReturnFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Permission, EPermission.UsersRead.GetValue())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetTenantId_WithTenantClaim_ShouldReturnTenantId()
    {
        // Arrange
        var expectedTenantId = "tenant-123";
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.TenantId, expectedTenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        Assert.Equal(expectedTenantId, result);
    }

    [Fact]
    public void GetTenantId_WithoutTenantClaim_ShouldReturnNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetTenantId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetOrganizationId_WithOrganizationClaim_ShouldReturnOrganizationId()
    {
        // Arrange
        var expectedOrgId = "org-456";
        var claims = new[]
        {
            new Claim(AuthConstants.Claims.Organization, expectedOrgId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetOrganizationId();

        // Assert
        Assert.Equal(expectedOrgId, result);
    }

    [Fact]
    public void GetOrganizationId_WithoutOrganizationClaim_ShouldReturnNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = principal.GetOrganizationId();

        // Assert
        Assert.Null(result);
    }
}


