using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

public class PermissionExtensionsTests
{
    #region GetValue Tests

    [Fact]
    public void GetValue_WithUsersReadPermission_ShouldReturnUsersRead()
    {
        // Arrange
        var permission = EPermission.UsersRead;

        // Act
        var result = permission.GetValue();

        // Assert
        result.Should().Be("users:read");
    }

    [Fact]
    public void GetValue_WithSystemAdminPermission_ShouldReturnSystemAdmin()
    {
        // Arrange
        var permission = EPermission.SystemAdmin;

        // Act
        var result = permission.GetValue();

        // Assert
        result.Should().Be("system:admin");
    }

    [Fact]
    public void GetValue_WithProvidersApprovePermission_ShouldReturnProvidersApprove()
    {
        // Arrange
        var permission = EPermission.ProvidersApprove;

        // Act
        var result = permission.GetValue();

        // Assert
        result.Should().Be("providers:approve");
    }

    #endregion

    #region GetModule Tests

    [Fact]
    public void GetModule_WithUsersPermission_ShouldReturnUsers()
    {
        // Arrange
        var permission = EPermission.UsersRead;

        // Act
        var result = permission.GetModule();

        // Assert
        result.Should().Be("users");
    }

    [Fact]
    public void GetModule_WithSystemPermission_ShouldReturnSystem()
    {
        // Arrange
        var permission = EPermission.SystemAdmin;

        // Act
        var result = permission.GetModule();

        // Assert
        result.Should().Be("system");
    }

    [Fact]
    public void GetModule_WithProvidersPermission_ShouldReturnProviders()
    {
        // Arrange
        var permission = EPermission.ProvidersCreate;

        // Act
        var result = permission.GetModule();

        // Assert
        result.Should().Be("providers");
    }

    [Fact]
    public void GetModule_WithOrdersPermission_ShouldReturnOrders()
    {
        // Arrange
        var permission = EPermission.OrdersRead;

        // Act
        var result = permission.GetModule();

        // Assert
        result.Should().Be("orders");
    }

    #endregion

    #region FromValue Tests

    [Fact]
    public void FromValue_WithValidPermissionValue_ShouldReturnCorrectPermission()
    {
        // Arrange
        var value = "users:read";

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().Be(EPermission.UsersRead);
    }

    [Fact]
    public void FromValue_WithCaseInsensitiveValue_ShouldReturnCorrectPermission()
    {
        // Arrange
        var value = "USERS:READ";

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().Be(EPermission.UsersRead);
    }

    [Fact]
    public void FromValue_WithInvalidPermissionValue_ShouldReturnNull()
    {
        // Arrange
        var value = "invalid:permission";

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromValue_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        string? value = null;

        // Act
        var result = PermissionExtensions.FromValue(value!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromValue_WithEmptyValue_ShouldReturnNull()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromValue_WithWhitespaceValue_ShouldReturnNull()
    {
        // Arrange
        var value = "   ";

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPermissionsByModule Tests

    [Fact]
    public void GetPermissionsByModule_WithUsersModule_ShouldReturnUsersPermissions()
    {
        // Arrange
        var module = "users";

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.UsersCreate);
        result.Should().Contain(EPermission.UsersUpdate);
        result.Should().Contain(EPermission.UsersDelete);
        result.Should().Contain(EPermission.UsersList);
        result.Should().Contain(EPermission.UsersProfile);
        result.Should().NotContain(EPermission.ProvidersRead);
        result.Should().NotContain(EPermission.SystemAdmin);
    }

    [Fact]
    public void GetPermissionsByModule_WithSystemModule_ShouldReturnSystemPermissions()
    {
        // Arrange
        var module = "system";

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().Contain(EPermission.SystemRead);
        result.Should().Contain(EPermission.SystemWrite);
        result.Should().Contain(EPermission.SystemAdmin);
        result.Should().NotContain(EPermission.UsersRead);
    }

    [Fact]
    public void GetPermissionsByModule_WithCaseInsensitiveModule_ShouldReturnPermissions()
    {
        // Arrange
        var module = "USERS";

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().Contain(EPermission.UsersRead);
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetPermissionsByModule_WithNonExistentModule_ShouldReturnEmptyList()
    {
        // Arrange
        var module = "nonexistent";

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPermissionsByModule_WithNullModule_ShouldReturnEmptyList()
    {
        // Arrange
        string? module = null;

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module!).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPermissionsByModule_WithEmptyModule_ShouldReturnEmptyList()
    {
        // Arrange
        var module = string.Empty;

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllModules Tests

    [Fact]
    public void GetAllModules_ShouldReturnAllDistinctModules()
    {
        // Act
        var result = PermissionExtensions.GetAllModules().ToList();

        // Assert
        result.Should().Contain("users");
        result.Should().Contain("system");
        result.Should().Contain("providers");
        result.Should().Contain("orders");
        result.Should().NotContain("unknown");
        result.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetAllModules_ShouldReturnSortedModules()
    {
        // Act
        var result = PermissionExtensions.GetAllModules().ToList();

        // Assert
        result.Should().BeInAscendingOrder();
    }

    #endregion

    #region IsAdminPermission Tests

    [Fact]
    public void IsAdminPermission_WithAdminModulePermission_ShouldReturnTrue()
    {
        // Arrange
        var permission = EPermission.AdminSystem;

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAdminPermission_WithSystemAdminPermission_ShouldReturnFalse()
    {
        // Arrange
        var permission = EPermission.SystemAdmin;

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        // system:admin doesn't have "admin" module
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdminPermission_WithUsersPermission_ShouldReturnFalse()
    {
        // Arrange
        var permission = EPermission.UsersRead;

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdminPermission_WithProvidersPermission_ShouldReturnFalse()
    {
        // Arrange
        var permission = EPermission.ProvidersCreate;

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
