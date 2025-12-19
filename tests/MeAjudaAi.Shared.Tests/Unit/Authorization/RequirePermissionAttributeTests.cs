using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Attributes;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Handlers;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para RequirePermissionAttribute e PermissionRequirement
/// para verificar as validações defensivas contra EPermission.None.
/// </summary>
public class RequirePermissionAttributeTests
{
    [Fact]
    public void RequirePermissionAttribute_WithEPermissionNone_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RequirePermissionAttribute(EPermission.None));

        exception.Message.Should().Contain("EPermission.None não é uma permissão válida para autorização");
        exception.ParamName.Should().Be("permission");
    }

    [Fact]
    public void PermissionRequirement_WithEPermissionNone_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PermissionRequirement(EPermission.None));

        exception.Message.Should().Contain("EPermission.None não é uma permissão válida para autorização");
        exception.ParamName.Should().Be("permission");
    }

    [Theory]
    [InlineData(EPermission.AdminSystem)]
    [InlineData(EPermission.UsersRead)]
    [InlineData(EPermission.UsersCreate)]
    [InlineData(EPermission.UsersUpdate)]
    [InlineData(EPermission.UsersDelete)]
    [InlineData(EPermission.ProvidersRead)]
    [InlineData(EPermission.ProvidersCreate)]
    [InlineData(EPermission.ProvidersUpdate)]
    [InlineData(EPermission.ProvidersDelete)]
    public void RequirePermissionAttribute_WithValidPermission_ShouldNotThrow(EPermission validPermission)
    {
        // Act & Assert
        var exception = Record.Exception(() => new RequirePermissionAttribute(validPermission));

        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(EPermission.AdminSystem)]
    [InlineData(EPermission.UsersRead)]
    [InlineData(EPermission.UsersCreate)]
    [InlineData(EPermission.UsersUpdate)]
    [InlineData(EPermission.UsersDelete)]
    [InlineData(EPermission.ProvidersRead)]
    [InlineData(EPermission.ProvidersCreate)]
    [InlineData(EPermission.ProvidersUpdate)]
    [InlineData(EPermission.ProvidersDelete)]
    public void PermissionRequirement_WithValidPermission_ShouldNotThrow(EPermission validPermission)
    {
        // Act & Assert
        var exception = Record.Exception(() => new PermissionRequirement(validPermission));

        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(EPermission.AdminSystem, "admin:system")]
    [InlineData(EPermission.UsersRead, "users:read")]
    [InlineData(EPermission.ProvidersCreate, "providers:create")]
    public void RequirePermissionAttribute_WithValidPermission_ShouldSetCorrectPolicy(EPermission permission, string expectedValue)
    {
        // Act
        var attribute = new RequirePermissionAttribute(permission);

        // Assert
        attribute.Permission.Should().Be(permission);
        attribute.PermissionValue.Should().Be(expectedValue);
        attribute.Policy.Should().Be($"RequirePermission:{expectedValue}");
    }

    [Theory]
    [InlineData(EPermission.AdminSystem, "admin:system")]
    [InlineData(EPermission.UsersRead, "users:read")]
    [InlineData(EPermission.ProvidersCreate, "providers:create")]
    public void PermissionRequirement_WithValidPermission_ShouldSetCorrectValues(EPermission permission, string expectedValue)
    {
        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
        requirement.PermissionValue.Should().Be(expectedValue);
    }
}
