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
        UserRoles.AllRoles.Should().HaveCount(13);
        UserRoles.AllRoles.Should().Contain(new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.ProviderManager,
            UserRoles.DocumentReviewer,
            UserRoles.CatalogManager,
            UserRoles.Operator,
            UserRoles.ReportViewer,
            UserRoles.Customer,
            UserRoles.Provider,
            UserRoles.ProviderStandard,
            UserRoles.ProviderSilver,
            UserRoles.ProviderGold,
            UserRoles.ProviderPlatinum
        });
    }

    [Fact]
    public void AdminRoles_ShouldContainOnlyAdminRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().HaveCount(7);
        UserRoles.AdminRoles.Should().Contain(new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.ProviderManager,
            UserRoles.DocumentReviewer,
            UserRoles.CatalogManager,
            UserRoles.Operator,
            UserRoles.ReportViewer
        });
    }

    [Fact]
    public void CustomerRoles_ShouldContainCustomerRole()
    {
        // Assert
        UserRoles.CustomerRoles.Should().HaveCount(1);
        UserRoles.CustomerRoles.Should().Contain(UserRoles.Customer);
    }

    [Fact]
    public void RoleConstants_ShouldHaveExpectedValues()
    {
        // Assert
        UserRoles.SuperAdmin.Should().Be(RoleConstants.SuperAdmin);
        UserRoles.Admin.Should().Be(RoleConstants.SystemAdmin);
        UserRoles.ProviderManager.Should().Be(RoleConstants.ProviderAdmin);
        UserRoles.DocumentReviewer.Should().Be(RoleConstants.LegacySystemAdmin);
        UserRoles.CatalogManager.Should().Be(RoleConstants.CatalogManager);
        UserRoles.Operator.Should().Be(RoleConstants.UserOperator);
        UserRoles.ReportViewer.Should().Be(RoleConstants.ReportViewer);
        UserRoles.Customer.Should().Be("customer");
        UserRoles.Provider.Should().Be(RoleConstants.Provider);
        UserRoles.ProviderStandard.Should().Be("meajudaai-provider-standard");
        UserRoles.ProviderSilver.Should().Be("meajudaai-provider-silver");
        UserRoles.ProviderGold.Should().Be("meajudaai-provider-gold");
        UserRoles.ProviderPlatinum.Should().Be("meajudaai-provider-platinum");
    }

    #endregion

    #region IsValidRole Tests

    [Theory]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.ProviderAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.CatalogManager)]
    [InlineData(RoleConstants.UserOperator)]
    [InlineData(RoleConstants.ReportViewer)]
    [InlineData("customer")]
    [InlineData(RoleConstants.Provider)]
    [InlineData("meajudaai-provider-standard")]
    [InlineData("meajudaai-provider-silver")]
    [InlineData("meajudaai-provider-gold")]
    [InlineData("meajudaai-provider-platinum")]
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
    [InlineData("MeAjudaAi-PrOvIdEr-AdMiN")]
    [InlineData("SYSTEM-ADMIN")]
    [InlineData("MeAjudaAi-CaTaLoG-MaNaGeR")]
    [InlineData("MeAjudaAi-PrOvIdEr-StAnDaRd")]
    [InlineData("MEAJUDAAI-PROVIDER-SILVER")]
    [InlineData("MeAjudaAi-PrOvIdEr-GoLd")]
    [InlineData("MeAjudaAi-PrOvIdEr-PlAtInUm")]
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
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.ProviderAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.CatalogManager)]
    [InlineData(RoleConstants.UserOperator)]
    [InlineData(RoleConstants.ReportViewer)]
    public void IsAdminRole_WithAdminRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("MEAJUDAAI-SYSTEM-ADMIN")]
    [InlineData("MeAjudaAi-PrOvIdEr-AdMiN")]
    [InlineData("SYSTEM-ADMIN")]
    [InlineData("MEAJUDAAI-REPORT-VIEWER")]
    public void IsAdminRole_WithAdminRoleDifferentCase_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("meajudaai-provider-standard")]
    [InlineData("meajudaai-provider-silver")]
    [InlineData("meajudaai-provider-gold")]
    [InlineData("meajudaai-provider-platinum")]
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
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.ProviderAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.CatalogManager)]
    [InlineData(RoleConstants.UserOperator)]
    [InlineData(RoleConstants.ReportViewer)]
    [InlineData("customer")]
    public void IsProviderRole_WithNonProviderRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(" ")]
    public void IsProviderRole_WithInvalidRole_ShouldReturnFalse(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProviderRole_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = UserRoles.IsProviderRole(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("MeAjudaAi-PrOvIdEr")]
    [InlineData("MeAjudaAi-PrOvIdEr-StAnDaRd")]
    [InlineData("MEAJUDAAI-PROVIDER-SILVER")]
    [InlineData("MeAjudaAi-PrOvIdEr-GoLd")]
    [InlineData("MeAjudaAi-PrOvIdEr-PlAtInUm")]
    public void IsProviderRole_WithValidRoleDifferentCase_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().BeTrue();
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
    public void CustomerRoles_ShouldBeSubsetOfAllRoles()
    {
        // Assert
        UserRoles.CustomerRoles.Should().BeSubsetOf(UserRoles.AllRoles);
    }

    [Fact]
    public void ProviderRoles_ShouldBeSubsetOfAllRoles()
    {
        // Assert
        UserRoles.ProviderRoles.Should().BeSubsetOf(UserRoles.AllRoles);
    }

    [Fact]
    public void ProviderRoles_ShouldContainOnlyProviderRoles()
    {
        // Assert
        UserRoles.ProviderRoles.Should().BeEquivalentTo(new[]
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
