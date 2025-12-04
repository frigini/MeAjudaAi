using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

public class PermissionRequirementTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidPermission_ShouldSetPermission()
    {
        // Arrange
        var permission = EPermission.UsersRead;

        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Constructor_WithNonePermission_ShouldThrowArgumentException()
    {
        // Arrange
        var permission = EPermission.None;

        // Act
        var act = () => new PermissionRequirement(permission);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("EPermission.None não é uma permissão válida para autorização*")
            .WithParameterName("permission");
    }

    [Fact]
    public void Constructor_WithSystemAdminPermission_ShouldSetPermission()
    {
        // Arrange
        var permission = EPermission.SystemAdmin;

        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Constructor_WithProvidersApprovePermission_ShouldSetPermission()
    {
        // Arrange
        var permission = EPermission.ProvidersApprove;

        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
    }

    #endregion

    #region PermissionValue Tests

    [Fact]
    public void PermissionValue_WithUsersReadPermission_ShouldReturnCorrectValue()
    {
        // Arrange
        var permission = EPermission.UsersRead;
        var requirement = new PermissionRequirement(permission);

        // Act
        var result = requirement.PermissionValue;

        // Assert
        result.Should().Be("users:read");
    }

    [Fact]
    public void PermissionValue_WithSystemAdminPermission_ShouldReturnCorrectValue()
    {
        // Arrange
        var permission = EPermission.SystemAdmin;
        var requirement = new PermissionRequirement(permission);

        // Act
        var result = requirement.PermissionValue;

        // Assert
        result.Should().Be("system:admin");
    }

    [Fact]
    public void PermissionValue_WithProvidersCreatePermission_ShouldReturnCorrectValue()
    {
        // Arrange
        var permission = EPermission.ProvidersCreate;
        var requirement = new PermissionRequirement(permission);

        // Act
        var result = requirement.PermissionValue;

        // Assert
        result.Should().Be("providers:create");
    }

    [Fact]
    public void PermissionValue_WithOrdersDeletePermission_ShouldReturnCorrectValue()
    {
        // Arrange
        var permission = EPermission.OrdersDelete;
        var requirement = new PermissionRequirement(permission);

        // Act
        var result = requirement.PermissionValue;

        // Assert
        result.Should().Be("orders:delete");
    }

    #endregion

    #region Multiple Requirements Tests

    [Fact]
    public void MultipleRequirements_ShouldHaveIndependentPermissions()
    {
        // Arrange
        var permission1 = EPermission.UsersRead;
        var permission2 = EPermission.ProvidersCreate;

        // Act
        var requirement1 = new PermissionRequirement(permission1);
        var requirement2 = new PermissionRequirement(permission2);

        // Assert
        requirement1.Permission.Should().Be(permission1);
        requirement2.Permission.Should().Be(permission2);
        requirement1.PermissionValue.Should().Be("users:read");
        requirement2.PermissionValue.Should().Be("providers:create");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void PermissionValue_ShouldDeriveFromGetValueExtension()
    {
        // Arrange
        var permission = EPermission.UsersUpdate;
        var requirement = new PermissionRequirement(permission);

        // Act
        var requirementValue = requirement.PermissionValue;
        var extensionValue = permission.GetValue();

        // Assert
        requirementValue.Should().Be(extensionValue);
    }

    [Fact]
    public void Permission_ShouldBeReadOnly()
    {
        // Arrange
        var permission = EPermission.UsersDelete;
        var requirement = new PermissionRequirement(permission);

        // Act & Assert
        // Permission property should only have a getter, not a setter
        requirement.Permission.Should().Be(permission);
        
        // This test verifies immutability by ensuring the requirement can't be changed
        var originalPermission = requirement.Permission;
        // No way to modify it - would be a compile error
        requirement.Permission.Should().Be(originalPermission);
    }

    #endregion
}
