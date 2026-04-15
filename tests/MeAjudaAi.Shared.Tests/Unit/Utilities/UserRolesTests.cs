using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class UserRolesTests
{
    #region Constants Tests

    [Fact]
    public void AllRoles_ShouldContainAllDefinedRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().HaveCount(21);
        UserRoles.AllRoles.Should().Contain(new[]
        {
            UserRoles.AdminLegacy,
            UserRoles.SystemAdmin,
            UserRoles.SuperAdmin,
            UserRoles.UserAdmin,
            UserRoles.UserOperator,
            UserRoles.User,
            UserRoles.ProviderAdmin,
            UserRoles.Provider,
            UserRoles.OrderAdmin,
            UserRoles.OrderOperator,
            UserRoles.ReportAdmin,
            UserRoles.ReportViewer,
            UserRoles.CatalogManager,
            UserRoles.LocationManager,
            UserRoles.Customer,
            UserRoles.ProviderStandard,
            UserRoles.ProviderSilver,
            UserRoles.ProviderGold,
            UserRoles.ProviderPlatinum,
            RoleConstants.LegacySystemAdmin,
            RoleConstants.LegacySuperAdmin
        });
    }

    [Fact]
    public void AdminRoles_ShouldContainOnlyAdminRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().HaveCount(11);
        UserRoles.AdminRoles.Should().Contain(new[]
        {
            UserRoles.AdminLegacy,
            UserRoles.SystemAdmin,
            UserRoles.SuperAdmin,
            UserRoles.UserAdmin,
            UserRoles.ProviderAdmin,
            UserRoles.OrderAdmin,
            UserRoles.ReportAdmin,
            UserRoles.CatalogManager,
            UserRoles.LocationManager,
            RoleConstants.LegacySystemAdmin,
            RoleConstants.LegacySuperAdmin
        });
    }

    [Fact]
    public void RoleConstants_ShouldHaveExpectedValues()
    {
        // Assert
        UserRoles.AdminLegacy.Should().Be(RoleConstants.Admin);
        UserRoles.SystemAdmin.Should().Be(RoleConstants.SystemAdmin);
        UserRoles.SuperAdmin.Should().Be(RoleConstants.SuperAdmin);
        UserRoles.UserAdmin.Should().Be(RoleConstants.UserAdmin);
        UserRoles.UserOperator.Should().Be(RoleConstants.UserOperator);
        UserRoles.User.Should().Be(RoleConstants.User);
        UserRoles.ProviderAdmin.Should().Be(RoleConstants.ProviderAdmin);
        UserRoles.Provider.Should().Be(RoleConstants.Provider);
        UserRoles.OrderAdmin.Should().Be(RoleConstants.OrderAdmin);
        UserRoles.OrderOperator.Should().Be(RoleConstants.OrderOperator);
        UserRoles.ReportAdmin.Should().Be(RoleConstants.ReportAdmin);
        UserRoles.ReportViewer.Should().Be(RoleConstants.ReportViewer);
        UserRoles.CatalogManager.Should().Be(RoleConstants.CatalogManager);
        UserRoles.LocationManager.Should().Be(RoleConstants.LocationManager);
        UserRoles.Customer.Should().Be("customer");
        UserRoles.ProviderStandard.Should().Be("meajudaai-provider-standard");
        UserRoles.ProviderSilver.Should().Be("meajudaai-provider-silver");
        UserRoles.ProviderGold.Should().Be("meajudaai-provider-gold");
        UserRoles.ProviderPlatinum.Should().Be("meajudaai-provider-platinum");
    }

    #endregion

    #region IsValidRole Tests

    [Theory]
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.UserAdmin)]
    [InlineData(RoleConstants.UserOperator)]
    [InlineData(RoleConstants.User)]
    [InlineData(RoleConstants.ProviderAdmin)]
    [InlineData(RoleConstants.Provider)]
    [InlineData(RoleConstants.OrderAdmin)]
    [InlineData(RoleConstants.OrderOperator)]
    [InlineData(RoleConstants.ReportAdmin)]
    [InlineData(RoleConstants.ReportViewer)]
    [InlineData(RoleConstants.CatalogManager)]
    [InlineData(RoleConstants.LocationManager)]
    [InlineData("customer")]
    [InlineData("meajudaai-provider-standard")]
    [InlineData("meajudaai-provider-silver")]
    [InlineData("meajudaai-provider-gold")]
    [InlineData("meajudaai-provider-platinum")]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.LegacySuperAdmin)]
    public void IsValidRole_WithValidRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("MeAjudaAi-SuPeR-AdMiN")]
    [InlineData("MEAJUDAAI-SYSTEM-ADMIN")]
    [InlineData("SYSTEM-ADMIN")]
    [InlineData("MEAJUDAAI-REPORT-VIEWER")]
    [InlineData("MeAjudaAi-PrOvIdEr-GoLd")]
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
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.UserAdmin)]
    [InlineData(RoleConstants.ProviderAdmin)]
    [InlineData(RoleConstants.OrderAdmin)]
    [InlineData(RoleConstants.ReportAdmin)]
    [InlineData(RoleConstants.CatalogManager)]
    [InlineData(RoleConstants.LocationManager)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.LegacySuperAdmin)]
    public void IsAdminRole_WithAdminRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("customer")]
    [InlineData(RoleConstants.User)]
    [InlineData(RoleConstants.Provider)]
    [InlineData(RoleConstants.ReportViewer)]
    [InlineData(RoleConstants.OrderOperator)]
    [InlineData(RoleConstants.UserOperator)]
    public void IsAdminRole_WithNonAdminRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsProviderRole Tests

    [Theory]
    [InlineData(RoleConstants.Provider)]
    [InlineData("meajudaai-provider-standard")]
    [InlineData("meajudaai-provider-silver")]
    [InlineData("meajudaai-provider-gold")]
    [InlineData("meajudaai-provider-platinum")]
    public void IsProviderRole_WithProviderRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData("customer")]
    public void IsProviderRole_WithNonProviderRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

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
    public void ProviderRoles_ShouldBeSubsetOfAllRoles()
    {
        // Assert
        UserRoles.ProviderRoles.Should().BeSubsetOf(UserRoles.AllRoles);
    }

    [Fact]
    public void ProviderRoles_ShouldContainExpectedTiers()
    {
        // Assert
        UserRoles.ProviderRoles.Should().Contain(new[]
        {
            UserRoles.Provider,
            UserRoles.ProviderStandard,
            UserRoles.ProviderSilver,
            UserRoles.ProviderGold,
            UserRoles.ProviderPlatinum
        });
    }

    #endregion
}
