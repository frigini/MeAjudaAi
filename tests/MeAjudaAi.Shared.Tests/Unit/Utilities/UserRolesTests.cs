using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class UserRolesTests
{
    [Theory]
    [InlineData(RoleConstants.ProviderGold, true)]
    [InlineData(RoleConstants.Customer, false)]
    [InlineData(RoleConstants.SystemAdmin, false)]
    public void IsProviderRole_Works(string role, bool expected)
        => UserRoles.IsProviderRole(role).Should().Be(expected);

    [Theory]
    [InlineData(RoleConstants.SystemAdmin, true)]
    [InlineData(RoleConstants.UserAdmin, true)]
    [InlineData(RoleConstants.Customer, false)]
    [InlineData(RoleConstants.Provider, false)]
    public void IsAdminRole_Works(string role, bool expected)
        => UserRoles.IsAdminRole(role).Should().Be(expected);

    [Theory]
    [InlineData(RoleConstants.Customer, true)]
    [InlineData("InvalidRole", false)]
    [InlineData("", false)]
    public void IsValidRole_Works(string role, bool expected)
        => UserRoles.IsValidRole(role).Should().Be(expected);

    [Fact]
    public void AllRoles_ShouldContain_EssentialRoles()
    {
        UserRoles.AllRoles.Should().Contain(RoleConstants.Customer);
        UserRoles.AllRoles.Should().Contain(RoleConstants.ProviderStandard);
        UserRoles.AllRoles.Should().Contain(RoleConstants.SystemAdmin);
    }
}
