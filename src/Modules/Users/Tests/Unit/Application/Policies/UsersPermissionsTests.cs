using FluentAssertions;
using MeAjudaAi.Modules.Users.Application.Policies;
using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Policies;

[Trait("Category", "Unit")]
public class UsersPermissionsTests
{
    [Fact]
    public void BasicUser_ShouldContainOnlyReadPermission()
    {
        // Arrange & Act
        var permissions = UsersPermissions.BasicUser;

        // Assert
        permissions.Should().NotBeNull();
        permissions.Should().HaveCount(1);
        permissions.Should().Contain(EPermission.UsersRead);
        permissions.Should().NotContain(EPermission.UsersUpdate);
        permissions.Should().NotContain(EPermission.UsersDelete);
    }

    [Fact]
    public void UserAdmin_ShouldContainReadAndUpdatePermissions()
    {
        // Arrange & Act
        var permissions = UsersPermissions.UserAdmin;

        // Assert
        permissions.Should().NotBeNull();
        permissions.Should().HaveCount(2);
        permissions.Should().Contain(EPermission.UsersRead);
        permissions.Should().Contain(EPermission.UsersUpdate);
        permissions.Should().NotContain(EPermission.UsersDelete);
        permissions.Should().NotContain(EPermission.AdminUsers);
    }

    [Fact]
    public void SystemAdmin_ShouldContainAllUserPermissions()
    {
        // Arrange & Act
        var permissions = UsersPermissions.SystemAdmin;

        // Assert
        permissions.Should().NotBeNull();
        permissions.Should().HaveCount(4);
        permissions.Should().Contain(EPermission.UsersRead);
        permissions.Should().Contain(EPermission.UsersUpdate);
        permissions.Should().Contain(EPermission.UsersDelete);
        permissions.Should().Contain(EPermission.AdminUsers);
    }

    [Fact]
    public void BasicUser_ShouldBeReadOnly()
    {
        // Arrange
        var permissions = UsersPermissions.BasicUser;

        // Act & Assert
        // Arrays are mutable in C#, but the field itself is readonly
        permissions.Should().NotBeNull();

        // Verify the field is static readonly via reflection
        var field = typeof(UsersPermissions).GetField(nameof(UsersPermissions.BasicUser));
        field.Should().NotBeNull();
        field!.IsStatic.Should().BeTrue();
        field.IsInitOnly.Should().BeTrue(); // readonly
    }

    [Fact]
    public void UserAdmin_ShouldBeReadOnly()
    {
        // Arrange & Act
        var field = typeof(UsersPermissions).GetField(nameof(UsersPermissions.UserAdmin));

        // Assert
        field.Should().NotBeNull();
        field!.IsStatic.Should().BeTrue();
        field.IsInitOnly.Should().BeTrue();
    }

    [Fact]
    public void SystemAdmin_ShouldBeReadOnly()
    {
        // Arrange & Act
        var field = typeof(UsersPermissions).GetField(nameof(UsersPermissions.SystemAdmin));

        // Assert
        field.Should().NotBeNull();
        field!.IsStatic.Should().BeTrue();
        field.IsInitOnly.Should().BeTrue();
    }

    [Fact]
    public void BasicUser_ShouldBeSubsetOfUserAdmin()
    {
        // Arrange
        var basicPermissions = UsersPermissions.BasicUser;
        var adminPermissions = UsersPermissions.UserAdmin;

        // Assert
        basicPermissions.Should().BeSubsetOf(adminPermissions);
    }

    [Fact]
    public void UserAdmin_ShouldBeSubsetOfSystemAdmin()
    {
        // Arrange
        var userAdminPermissions = UsersPermissions.UserAdmin;
        var systemAdminPermissions = UsersPermissions.SystemAdmin;

        // Assert
        userAdminPermissions.Should().BeSubsetOf(systemAdminPermissions);
    }

    [Fact]
    public void BasicUser_ShouldBeSubsetOfSystemAdmin()
    {
        // Arrange
        var basicPermissions = UsersPermissions.BasicUser;
        var systemAdminPermissions = UsersPermissions.SystemAdmin;

        // Assert
        basicPermissions.Should().BeSubsetOf(systemAdminPermissions);
    }

    [Fact]
    public void AllPermissionArrays_ShouldNotBeNull()
    {
        // Assert
        UsersPermissions.BasicUser.Should().NotBeNull();
        UsersPermissions.UserAdmin.Should().NotBeNull();
        UsersPermissions.SystemAdmin.Should().NotBeNull();
    }

    [Fact]
    public void AllPermissionArrays_ShouldNotBeEmpty()
    {
        // Assert
        UsersPermissions.BasicUser.Should().NotBeEmpty();
        UsersPermissions.UserAdmin.Should().NotBeEmpty();
        UsersPermissions.SystemAdmin.Should().NotBeEmpty();
    }

    [Fact]
    public void AllPermissionArrays_ShouldContainUniqueValues()
    {
        // Assert
        UsersPermissions.BasicUser.Should().OnlyHaveUniqueItems();
        UsersPermissions.UserAdmin.Should().OnlyHaveUniqueItems();
        UsersPermissions.SystemAdmin.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(nameof(UsersPermissions.BasicUser), false)]
    [InlineData(nameof(UsersPermissions.UserAdmin), false)]
    [InlineData(nameof(UsersPermissions.SystemAdmin), true)]
    public void DeletePermission_ShouldOnlyBeInSystemAdmin(string groupName, bool shouldContainDelete)
    {
        // Arrange
        var permissions = groupName switch
        {
            nameof(UsersPermissions.BasicUser) => UsersPermissions.BasicUser,
            nameof(UsersPermissions.UserAdmin) => UsersPermissions.UserAdmin,
            nameof(UsersPermissions.SystemAdmin) => UsersPermissions.SystemAdmin,
            _ => throw new ArgumentException($"Unknown group: {groupName}")
        };

        // Assert
        if (shouldContainDelete)
            permissions.Should().Contain(EPermission.UsersDelete);
        else
            permissions.Should().NotContain(EPermission.UsersDelete);
    }
}
