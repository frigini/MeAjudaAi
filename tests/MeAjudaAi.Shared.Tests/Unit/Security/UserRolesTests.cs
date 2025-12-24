using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Security;

public class UserRolesTests
{
    #region Constants Tests

    [Fact]
    public void AllRoles_ShouldContainAllDefinedRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().HaveCount(6);
        UserRoles.AllRoles.Should().Contain(new[]
        {
            UserRoles.User,
            UserRoles.Admin,
            UserRoles.SuperAdmin,
            UserRoles.ServiceProvider,
            UserRoles.Customer,
            UserRoles.Moderator
        });
    }

    [Fact]
    public void AdminRoles_ShouldContainOnlyAdminRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().HaveCount(2);
        UserRoles.AdminRoles.Should().Contain(new[]
        {
            UserRoles.Admin,
            UserRoles.SuperAdmin
        });
    }

    [Fact]
    public void BasicRoles_ShouldContainOnlyBasicRoles()
    {
        // Assert
        UserRoles.BasicRoles.Should().HaveCount(3);
        UserRoles.BasicRoles.Should().Contain(new[]
        {
            UserRoles.User,
            UserRoles.Customer,
            UserRoles.ServiceProvider
        });
    }

    [Fact]
    public void RoleConstants_ShouldHaveExpectedValues()
    {
        // Assert
        UserRoles.User.Should().Be("user");
        UserRoles.Admin.Should().Be("admin");
        UserRoles.SuperAdmin.Should().Be("super-admin");
        UserRoles.ServiceProvider.Should().Be("service-provider");
        UserRoles.Customer.Should().Be("customer");
        UserRoles.Moderator.Should().Be("moderator");
    }

    #endregion

    #region IsValidRole Tests

    [Theory]
    [InlineData("user")]
    [InlineData("admin")]
    [InlineData("super-admin")]
    [InlineData("service-provider")]
    [InlineData("customer")]
    [InlineData("moderator")]
    public void IsValidRole_WithValidRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("USER")]
    [InlineData("ADMIN")]
    [InlineData("Super-Admin")]
    [InlineData("SERVICE-PROVIDER")]
    public void IsValidRole_WithValidRoleDifferentCase_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("guest")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("user123")]
    [InlineData("admin-user")]
    public void IsValidRole_WithInvalidRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidRole_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = UserRoles.IsValidRole(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsAdminRole Tests

    [Theory]
    [InlineData("admin")]
    [InlineData("super-admin")]
    public void IsAdminRole_WithAdminRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("Super-Admin")]
    [InlineData("SUPER-ADMIN")]
    public void IsAdminRole_WithAdminRoleDifferentCase_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("user")]
    [InlineData("customer")]
    [InlineData("service-provider")]
    [InlineData("moderator")]
    public void IsAdminRole_WithNonAdminRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(" ")]
    public void IsAdminRole_WithInvalidRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdminRole_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = UserRoles.IsAdminRole(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void AdminRoles_ShouldBeSubsetOfAllRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().BeSubsetOf(UserRoles.AllRoles);
    }

    [Fact]
    public void BasicRoles_ShouldBeSubsetOfAllRoles()
    {
        // Assert
        UserRoles.BasicRoles.Should().BeSubsetOf(UserRoles.AllRoles);
    }

    [Fact]
    public void AdminRoles_ShouldNotOverlapWithBasicRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().NotIntersectWith(UserRoles.BasicRoles);
    }

    #endregion
}
