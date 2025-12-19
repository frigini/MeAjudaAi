using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para o enum EPermissions e suas extensões.
/// </summary>
public class PermissionTests
{
    [Fact]
    public void EPermissions_ShouldHaveCorrectDisplayAttributes()
    {
        // Arrange & Act
        var adminSystemAttribute = EPermission.AdminSystem.GetType()
            .GetField(EPermission.AdminSystem.ToString())
            ?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(adminSystemAttribute);
        Assert.Equal("admin:system", adminSystemAttribute.Name);
    }

    [Theory]
    [InlineData(EPermission.AdminSystem, "admin:system")]
    [InlineData(EPermission.UsersRead, "users:read")]
    [InlineData(EPermission.UsersCreate, "users:create")]
    [InlineData(EPermission.UsersUpdate, "users:update")]
    [InlineData(EPermission.UsersDelete, "users:delete")]
    public void GetValue_ShouldReturnCorrectStringValue(EPermission permission, string expectedValue)
    {
        // Act
        var result = permission.GetValue();

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [InlineData(EPermission.AdminSystem, "admin")]
    [InlineData(EPermission.UsersRead, "users")]
    [InlineData(EPermission.AdminUsers, "admin")]
    public void GetModule_ShouldReturnCorrectModuleName(EPermission permission, string expectedModule)
    {
        // Act
        var result = permission.GetModule();

        // Assert
        Assert.Equal(expectedModule, result);
    }

    [Theory]
    [InlineData("admin:system", EPermission.AdminSystem)]
    [InlineData("users:read", EPermission.UsersRead)]
    [InlineData("users:create", EPermission.UsersCreate)]
    public void FromValue_WithValidValue_ShouldReturnCorrectPermission(string value, EPermission expectedPermission)
    {
        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        Assert.True(result.HasValue);
        Assert.Equal(expectedPermission, result.Value);
    }

    [Theory]
    [InlineData("invalid:permission")]
    [InlineData("")]
    [InlineData(null)]
    public void FromValue_WithInvalidValue_ShouldReturnNull(string invalidValue)
    {
        // Act
        var result = PermissionExtensions.FromValue(invalidValue);

        // Assert
        Assert.False(result.HasValue);
    }

    [Theory]
    [InlineData("system")]
    [InlineData("users")]
    [InlineData("providers")]
    [InlineData("orders")]
    [InlineData("reports")]
    public void GetPermissionsByModule_ShouldReturnOnlyModulePermissions(string moduleName)
    {
        // Act
        var result = PermissionExtensions.GetPermissionsByModule(moduleName);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, permission => Assert.Equal(moduleName, permission.GetModule()));
    }

    [Fact]
    public void GetPermissionsByModule_WithInvalidModule_ShouldReturnEmpty()
    {
        // Act
        var result = PermissionExtensions.GetPermissionsByModule("InvalidModule");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AllPermissions_ShouldHaveUniqueValues()
    {
        // Arrange
        var allPermissions = Enum.GetValues<EPermission>();

        // Act
        var permissionValues = allPermissions.Select(p => p.GetValue()).ToList();

        // Assert
        Assert.Equal(permissionValues.Count, permissionValues.Distinct().Count());
    }

    [Fact]
    public void AllPermissions_ShouldHaveValidModuleNames()
    {
        // Arrange
        var allPermissions = Enum.GetValues<EPermission>();
        var validModules = new[] { "system", "users", "providers", "orders", "reports", "admin" };

        // Act & Assert
        foreach (var permission in allPermissions)
        {
            var module = permission.GetModule();
            Assert.Contains(module, validModules);
        }
    }
}
