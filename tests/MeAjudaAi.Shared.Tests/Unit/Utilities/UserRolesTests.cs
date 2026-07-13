using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class UserRolesTests
{
    [Theory]
    [InlineData(RoleConstants.Admin, true)]
    [InlineData(RoleConstants.SystemAdmin, true)]
    [InlineData(RoleConstants.SuperAdmin, true)]
    [InlineData(RoleConstants.UserAdmin, true)]
    [InlineData(RoleConstants.UserOperator, true)]
    [InlineData(RoleConstants.User, true)]
    [InlineData(RoleConstants.ProviderAdmin, true)]
    [InlineData(RoleConstants.Provider, true)]
    [InlineData(RoleConstants.OrderAdmin, true)]
    [InlineData(RoleConstants.OrderOperator, true)]
    [InlineData(RoleConstants.ReportAdmin, true)]
    [InlineData(RoleConstants.ReportViewer, true)]
    [InlineData(RoleConstants.CatalogManager, true)]
    [InlineData(RoleConstants.LocationManager, true)]
    [InlineData(RoleConstants.Customer, true)]
    [InlineData(RoleConstants.ProviderStandard, true)]
    [InlineData(RoleConstants.ProviderSilver, true)]
    [InlineData(RoleConstants.ProviderGold, true)]
    [InlineData(RoleConstants.ProviderPlatinum, true)]
    [InlineData(RoleConstants.LegacySystemAdmin, true)]
    [InlineData(RoleConstants.LegacySuperAdmin, true)]
    [InlineData("InvalidRole", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidRole_WithVariousRoles_ReturnsExpected(string? role, bool expected)
    {
        // Act
        var result = UserRoles.IsValidRole(role!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("admin", true)]
    [InlineData("ADMIN", true)]
    [InlineData("MeAjudaAi-System-Admin", true)]
    [InlineData("customer", true)]
    [InlineData("meajudaai-provider", true)]
    [InlineData("MeAjudaAi-Provider-Gold", true)]
    public void IsValidRole_IsCaseInsensitive(string role, bool expected)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(RoleConstants.Admin, true)]
    [InlineData(RoleConstants.SystemAdmin, true)]
    [InlineData(RoleConstants.SuperAdmin, true)]
    [InlineData(RoleConstants.UserAdmin, true)]
    [InlineData(RoleConstants.ProviderAdmin, true)]
    [InlineData(RoleConstants.OrderAdmin, true)]
    [InlineData(RoleConstants.ReportAdmin, true)]
    [InlineData(RoleConstants.CatalogManager, true)]
    [InlineData(RoleConstants.LocationManager, true)]
    [InlineData(RoleConstants.LegacySystemAdmin, true)]
    [InlineData(RoleConstants.LegacySuperAdmin, true)]
    [InlineData(RoleConstants.Customer, false)]
    [InlineData(RoleConstants.User, false)]
    [InlineData(RoleConstants.UserOperator, false)]
    [InlineData(RoleConstants.Provider, false)]
    [InlineData(RoleConstants.OrderOperator, false)]
    [InlineData(RoleConstants.ReportViewer, false)]
    [InlineData(RoleConstants.ProviderStandard, false)]
    [InlineData("InvalidRole", false)]
    [InlineData(null, false)]
    public void IsAdminRole_WithVariousRoles_ReturnsExpected(string? role, bool expected)
    {
        // Act
        var result = UserRoles.IsAdminRole(role!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("ADMIN", true)]
    [InlineData("MeAjudaAi-Super-Admin", true)]
    [InlineData("CUSTOMER", false)]
    public void IsAdminRole_IsCaseInsensitive(string role, bool expected)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(RoleConstants.Provider, true)]
    [InlineData(RoleConstants.ProviderStandard, true)]
    [InlineData(RoleConstants.ProviderSilver, true)]
    [InlineData(RoleConstants.ProviderGold, true)]
    [InlineData(RoleConstants.ProviderPlatinum, true)]
    [InlineData(RoleConstants.Admin, false)]
    [InlineData(RoleConstants.SystemAdmin, false)]
    [InlineData(RoleConstants.Customer, false)]
    [InlineData(RoleConstants.User, false)]
    [InlineData("InvalidRole", false)]
    [InlineData(null, false)]
    public void IsProviderRole_WithVariousRoles_ReturnsExpected(string? role, bool expected)
    {
        // Act
        var result = UserRoles.IsProviderRole(role!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("MEAJUDAAI-PROVIDER", true)]
    [InlineData("MeAjudaAi-Provider-Gold", true)]
    [InlineData("CUSTOMER", false)]
    public void IsProviderRole_IsCaseInsensitive(string role, bool expected)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void AllRoles_ShouldContainAllDefinedRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().Contain(RoleConstants.Admin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.SystemAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.SuperAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.UserAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.UserOperator);
        UserRoles.AllRoles.Should().Contain(RoleConstants.User);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.Provider);
        UserRoles.AllRoles.Should().Contain(RoleConstants.OrderAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.OrderOperator);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ReportAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ReportViewer);
        UserRoles.AllRoles.Should().Contain(RoleConstants.CatalogManager);
        UserRoles.AllRoles.Should().Contain(RoleConstants.LocationManager);
        UserRoles.AllRoles.Should().Contain(RoleConstants.Customer);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderStandard);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderSilver);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderGold);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderPlatinum);
        UserRoles.AllRoles.Should().Contain(RoleConstants.LegacySystemAdmin);
        UserRoles.AllRoles.Should().Contain(RoleConstants.LegacySuperAdmin);
    }

    [Fact]
    public void AllRoles_ShouldNotContainDuplicates()
    {
        // Assert
        UserRoles.AllRoles.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AdminRoles_ShouldContainAllAdminRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().Contain(RoleConstants.Admin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.SystemAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.SuperAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.UserAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.ProviderAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.OrderAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.ReportAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.CatalogManager);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.LocationManager);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.LegacySystemAdmin);
        UserRoles.AdminRoles.Should().Contain(RoleConstants.LegacySuperAdmin);
    }

    [Fact]
    public void AdminRoles_ShouldNotContainNonAdminRoles()
    {
        // Assert
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.User);
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.UserOperator);
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.Customer);
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.Provider);
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.OrderOperator);
        UserRoles.AdminRoles.Should().NotContain(RoleConstants.ReportViewer);
    }

    [Fact]
    public void ProviderRoles_ShouldContainAllProviderTiers()
    {
        // Assert
        UserRoles.ProviderRoles.Should().Contain(RoleConstants.Provider);
        UserRoles.ProviderRoles.Should().Contain(RoleConstants.ProviderStandard);
        UserRoles.ProviderRoles.Should().Contain(RoleConstants.ProviderSilver);
        UserRoles.ProviderRoles.Should().Contain(RoleConstants.ProviderGold);
        UserRoles.ProviderRoles.Should().Contain(RoleConstants.ProviderPlatinum);
    }

    [Fact]
    public void ProviderRoles_ShouldNotContainNonProviderRoles()
    {
        // Assert
        UserRoles.ProviderRoles.Should().NotContain(RoleConstants.Admin);
        UserRoles.ProviderRoles.Should().NotContain(RoleConstants.Customer);
        UserRoles.ProviderRoles.Should().NotContain(RoleConstants.User);
    }

    [Fact]
    public void AllRoles_ShouldBeSupersetOfAdminRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().Contain(UserRoles.AdminRoles);
    }

    [Fact]
    public void AllRoles_ShouldBeSupersetOfProviderRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().Contain(UserRoles.ProviderRoles);
    }

    [Fact]
    public void AdminRoles_And_ProviderRoles_ShouldNotOverlap()
    {
        // Assert
        UserRoles.AdminRoles.Should().NotContain(UserRoles.ProviderRoles);
    }
}
