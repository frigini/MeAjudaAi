using FluentAssertions;
using MeAjudaAi.Shared.Utilities;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class UserRolesTests
{
    #region Constants Tests

    [Fact]
    public void AllRoles_ShouldContainAllDefinedRoles()
    {
        // Assert
        UserRoles.AllRoles.Should().HaveCount(11);
        UserRoles.AllRoles.Should().Contain(new[]
        {
            UserRoles.Admin,
            UserRoles.ProviderManager,
            UserRoles.DocumentReviewer,
            UserRoles.CatalogManager,
            UserRoles.Operator,
            UserRoles.Viewer,
            UserRoles.Customer,
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
        UserRoles.AdminRoles.Should().HaveCount(5);
        UserRoles.AdminRoles.Should().Contain(new[]
        {
            UserRoles.Admin,
            UserRoles.ProviderManager,
            UserRoles.DocumentReviewer,
            UserRoles.CatalogManager,
            UserRoles.Operator
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
        UserRoles.Admin.Should().Be("admin");
        UserRoles.ProviderManager.Should().Be("provider-manager");
        UserRoles.DocumentReviewer.Should().Be("document-reviewer");
        UserRoles.CatalogManager.Should().Be("catalog-manager");
        UserRoles.Operator.Should().Be("operator");
        UserRoles.Viewer.Should().Be("viewer");
        UserRoles.Customer.Should().Be("customer");
        UserRoles.ProviderStandard.Should().Be("provider-standard");
        UserRoles.ProviderSilver.Should().Be("provider-silver");
        UserRoles.ProviderGold.Should().Be("provider-gold");
        UserRoles.ProviderPlatinum.Should().Be("provider-platinum");
    }

    #endregion

    #region IsValidRole Tests

    [Theory]
    [InlineData("admin")]
    [InlineData("provider-manager")]
    [InlineData("document-reviewer")]
    [InlineData("catalog-manager")]
    [InlineData("operator")]
    [InlineData("viewer")]
    [InlineData("customer")]
    [InlineData("provider-standard")]
    [InlineData("provider-silver")]
    [InlineData("provider-gold")]
    [InlineData("provider-platinum")]
    public void IsValidRole_WithValidRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsValidRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("Provider-Manager")]
    [InlineData("DOCUMENT-REVIEWER")]
    [InlineData("Catalog-Manager")]
    [InlineData("Provider-Standard")]
    [InlineData("PROVIDER-SILVER")]
    [InlineData("provider-gold")]
    [InlineData("PrOvIdEr-PlAtInUm")]
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
    [InlineData("provider-manager")]
    [InlineData("document-reviewer")]
    [InlineData("catalog-manager")]
    [InlineData("operator")]
    public void IsAdminRole_WithAdminRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("Provider-Manager")]
    [InlineData("DOCUMENT-REVIEWER")]
    public void IsAdminRole_WithAdminRoleDifferentCase_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsAdminRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("viewer")]
    [InlineData("provider-standard")]
    [InlineData("provider-silver")]
    [InlineData("provider-gold")]
    [InlineData("provider-platinum")]
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
    [InlineData("provider-standard")]
    [InlineData("provider-silver")]
    [InlineData("provider-gold")]
    [InlineData("provider-platinum")]
    public void IsProviderRole_WithProviderRole_ShouldReturnTrue(string role)
    {
        // Act
        var result = UserRoles.IsProviderRole(role);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("provider-manager")]
    [InlineData("document-reviewer")]
    [InlineData("catalog-manager")]
    [InlineData("operator")]
    [InlineData("customer")]
    [InlineData("viewer")]
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
    [InlineData("Provider-Standard")]
    [InlineData("PROVIDER-SILVER")]
    [InlineData("provider-GOLD")]
    [InlineData("PrOvIdEr-PlAtInUm")]
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
            UserRoles.ProviderStandard,
            UserRoles.ProviderSilver,
            UserRoles.ProviderGold,
            UserRoles.ProviderPlatinum
        });
    }

    #endregion
}
