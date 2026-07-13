using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Authorization.Handlers;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Handlers;

public class PermissionRequirementTests
{
    [Theory]
    [InlineData(EPermission.UsersRead)]
    [InlineData(EPermission.UsersCreate)]
    [InlineData(EPermission.UsersUpdate)]
    [InlineData(EPermission.UsersDelete)]
    [InlineData(EPermission.ProvidersRead)]
    [InlineData(EPermission.ProvidersCreate)]
    [InlineData(EPermission.ProvidersApprove)]
    [InlineData(EPermission.SystemAdmin)]
    [InlineData(EPermission.AdminSystem)]
    [InlineData(EPermission.BookingsRead)]
    [InlineData(EPermission.BookingsCancel)]
    [InlineData(EPermission.PaymentsManage)]
    [InlineData(EPermission.CommunicationsSend)]
    [InlineData(EPermission.RatingsModerate)]
    [InlineData(EPermission.SearchRead)]
    [InlineData(EPermission.DocumentsUpload)]
    [InlineData(EPermission.ServiceCatalogsManage)]
    [InlineData(EPermission.LocationsRead)]
    [InlineData(EPermission.ReportsExport)]
    public void Constructor_WithValidPermission_ShouldSetPermission(EPermission permission)
    {
        // Arrange
        // (parameterized)

        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Constructor_WithNonePermission_ShouldThrowArgumentException()
    {
        // Arrange
        // (none)

        // Act
        var act = () => new PermissionRequirement(EPermission.None);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("EPermission.None não é uma permissão válida para autorização*")
            .WithParameterName("permission");
    }

    [Theory]
    [InlineData(EPermission.UsersRead, "users:read")]
    [InlineData(EPermission.UsersCreate, "users:create")]
    [InlineData(EPermission.SystemAdmin, "system:admin")]
    [InlineData(EPermission.ProvidersCreate, "providers:create")]
    [InlineData(EPermission.BookingsCancel, "bookings:cancel")]
    [InlineData(EPermission.PaymentsManage, "payments:manage")]
    [InlineData(EPermission.AdminSystem, "admin:system")]
    [InlineData(EPermission.ServiceCatalogsManage, "service-catalogs:manage")]
    [InlineData(EPermission.DocumentsUpload, "documents:upload")]
    public void PermissionValue_ShouldDeriveFromGetValueExtension(EPermission permission, string expectedValue)
    {
        // Arrange
        // (parameterized)

        // Act
        var requirement = new PermissionRequirement(permission);

        // Assert
        requirement.PermissionValue.Should().Be(expectedValue);
        requirement.PermissionValue.Should().Be(permission.GetValue());
    }

    [Fact]
    public void MultipleRequirements_ShouldHaveIndependentPermissions()
    {
        // Arrange
        var requirement1 = new PermissionRequirement(EPermission.UsersRead);
        var requirement2 = new PermissionRequirement(EPermission.ProvidersCreate);

        // Act
        // (already created in Arrange)

        // Assert
        requirement1.Permission.Should().Be(EPermission.UsersRead);
        requirement2.Permission.Should().Be(EPermission.ProvidersCreate);
        requirement1.PermissionValue.Should().Be("users:read");
        requirement2.PermissionValue.Should().Be("providers:create");
    }
}
