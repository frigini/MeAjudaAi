using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Extensions;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Extensions;

public class PermissionExtensionsTests
{
    #region GetValue Tests

    [Theory]
    [InlineData(EPermission.None, "system:none")]
    [InlineData(EPermission.SystemRead, "system:read")]
    [InlineData(EPermission.SystemWrite, "system:write")]
    [InlineData(EPermission.SystemAdmin, "system:admin")]
    [InlineData(EPermission.UsersRead, "users:read")]
    [InlineData(EPermission.UsersCreate, "users:create")]
    [InlineData(EPermission.UsersUpdate, "users:update")]
    [InlineData(EPermission.UsersDelete, "users:delete")]
    [InlineData(EPermission.UsersList, "users:list")]
    [InlineData(EPermission.UsersProfile, "users:profile")]
    [InlineData(EPermission.UsersRegister, "users:register")]
    [InlineData(EPermission.ProvidersRead, "providers:read")]
    [InlineData(EPermission.ProvidersCreate, "providers:create")]
    [InlineData(EPermission.ProvidersUpdate, "providers:update")]
    [InlineData(EPermission.ProvidersDelete, "providers:delete")]
    [InlineData(EPermission.ProvidersList, "providers:list")]
    [InlineData(EPermission.ProvidersApprove, "providers:approve")]
    [InlineData(EPermission.ProvidersRegister, "providers:register")]
    [InlineData(EPermission.ProvidersUploadDocuments, "providers:upload-documents")]
    [InlineData(EPermission.ProvidersManageTier, "providers:manage-tier")]
    [InlineData(EPermission.ServiceCatalogsRead, "service-catalogs:read")]
    [InlineData(EPermission.ServiceCatalogsManage, "service-catalogs:manage")]
    [InlineData(EPermission.LocationsRead, "locations:read")]
    [InlineData(EPermission.LocationsManage, "locations:manage")]
    [InlineData(EPermission.BookingsRead, "bookings:read")]
    [InlineData(EPermission.BookingsCreate, "bookings:create")]
    [InlineData(EPermission.BookingsUpdate, "bookings:update")]
    [InlineData(EPermission.BookingsCancel, "bookings:cancel")]
    [InlineData(EPermission.BookingsList, "bookings:list")]
    [InlineData(EPermission.BookingsManage, "bookings:manage")]
    [InlineData(EPermission.PaymentsRead, "payments:read")]
    [InlineData(EPermission.PaymentsManage, "payments:manage")]
    [InlineData(EPermission.PaymentsCheckout, "payments:checkout")]
    [InlineData(EPermission.CommunicationsRead, "communications:read")]
    [InlineData(EPermission.CommunicationsSend, "communications:send")]
    [InlineData(EPermission.CommunicationsManage, "communications:manage")]
    [InlineData(EPermission.RatingsRead, "ratings:read")]
    [InlineData(EPermission.RatingsCreate, "ratings:create")]
    [InlineData(EPermission.RatingsModerate, "ratings:moderate")]
    [InlineData(EPermission.SearchRead, "search:read")]
    [InlineData(EPermission.SearchManage, "search:manage")]
    [InlineData(EPermission.DocumentsRead, "documents:read")]
    [InlineData(EPermission.DocumentsUpload, "documents:upload")]
    [InlineData(EPermission.DocumentsVerify, "documents:verify")]
    [InlineData(EPermission.DocumentsDelete, "documents:delete")]
    [InlineData(EPermission.ReportsView, "reports:view")]
    [InlineData(EPermission.ReportsExport, "reports:export")]
    [InlineData(EPermission.ReportsCreate, "reports:create")]
    [InlineData(EPermission.ReportsAdmin, "reports:admin")]
    [InlineData(EPermission.AdminSystem, "admin:system")]
    [InlineData(EPermission.AdminUsers, "admin:users")]
    [InlineData(EPermission.AdminReports, "admin:reports")]
    public void GetValue_ShouldReturnCorrectStringValue(EPermission permission, string expectedValue)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = permission.GetValue();

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region GetModule Tests

    [Theory]
    [InlineData(EPermission.SystemRead, "system")]
    [InlineData(EPermission.SystemWrite, "system")]
    [InlineData(EPermission.SystemAdmin, "system")]
    [InlineData(EPermission.UsersRead, "users")]
    [InlineData(EPermission.UsersCreate, "users")]
    [InlineData(EPermission.UsersUpdate, "users")]
    [InlineData(EPermission.UsersDelete, "users")]
    [InlineData(EPermission.UsersList, "users")]
    [InlineData(EPermission.UsersProfile, "users")]
    [InlineData(EPermission.UsersRegister, "users")]
    [InlineData(EPermission.ProvidersRead, "providers")]
    [InlineData(EPermission.ProvidersCreate, "providers")]
    [InlineData(EPermission.ProvidersUpdate, "providers")]
    [InlineData(EPermission.ProvidersDelete, "providers")]
    [InlineData(EPermission.ProvidersList, "providers")]
    [InlineData(EPermission.ProvidersApprove, "providers")]
    [InlineData(EPermission.ProvidersRegister, "providers")]
    [InlineData(EPermission.ProvidersUploadDocuments, "providers")]
    [InlineData(EPermission.ProvidersManageTier, "providers")]
    [InlineData(EPermission.ServiceCatalogsRead, "service-catalogs")]
    [InlineData(EPermission.ServiceCatalogsManage, "service-catalogs")]
    [InlineData(EPermission.LocationsRead, "locations")]
    [InlineData(EPermission.LocationsManage, "locations")]
    [InlineData(EPermission.BookingsRead, "bookings")]
    [InlineData(EPermission.BookingsCreate, "bookings")]
    [InlineData(EPermission.BookingsUpdate, "bookings")]
    [InlineData(EPermission.BookingsCancel, "bookings")]
    [InlineData(EPermission.BookingsList, "bookings")]
    [InlineData(EPermission.BookingsManage, "bookings")]
    [InlineData(EPermission.PaymentsRead, "payments")]
    [InlineData(EPermission.PaymentsManage, "payments")]
    [InlineData(EPermission.PaymentsCheckout, "payments")]
    [InlineData(EPermission.CommunicationsRead, "communications")]
    [InlineData(EPermission.CommunicationsSend, "communications")]
    [InlineData(EPermission.CommunicationsManage, "communications")]
    [InlineData(EPermission.RatingsRead, "ratings")]
    [InlineData(EPermission.RatingsCreate, "ratings")]
    [InlineData(EPermission.RatingsModerate, "ratings")]
    [InlineData(EPermission.SearchRead, "search")]
    [InlineData(EPermission.SearchManage, "search")]
    [InlineData(EPermission.DocumentsRead, "documents")]
    [InlineData(EPermission.DocumentsUpload, "documents")]
    [InlineData(EPermission.DocumentsVerify, "documents")]
    [InlineData(EPermission.DocumentsDelete, "documents")]
    [InlineData(EPermission.ReportsView, "reports")]
    [InlineData(EPermission.ReportsExport, "reports")]
    [InlineData(EPermission.ReportsCreate, "reports")]
    [InlineData(EPermission.ReportsAdmin, "reports")]
    [InlineData(EPermission.AdminSystem, "admin")]
    [InlineData(EPermission.AdminUsers, "admin")]
    [InlineData(EPermission.AdminReports, "admin")]
    public void GetModule_ShouldReturnCorrectModuleName(EPermission permission, string expectedModule)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = permission.GetModule();

        // Assert
        result.Should().Be(expectedModule);
    }

    #endregion

    #region FromValue Tests

    [Theory]
    [InlineData("users:read", EPermission.UsersRead)]
    [InlineData("users:create", EPermission.UsersCreate)]
    [InlineData("system:admin", EPermission.SystemAdmin)]
    [InlineData("providers:approve", EPermission.ProvidersApprove)]
    [InlineData("bookings:cancel", EPermission.BookingsCancel)]
    [InlineData("payments:checkout", EPermission.PaymentsCheckout)]
    [InlineData("communications:send", EPermission.CommunicationsSend)]
    [InlineData("ratings:moderate", EPermission.RatingsModerate)]
    [InlineData("search:read", EPermission.SearchRead)]
    [InlineData("documents:upload", EPermission.DocumentsUpload)]
    [InlineData("admin:system", EPermission.AdminSystem)]
    [InlineData("service-catalogs:manage", EPermission.ServiceCatalogsManage)]
    [InlineData("locations:read", EPermission.LocationsRead)]
    [InlineData("reports:export", EPermission.ReportsExport)]
    public void FromValue_WithValidValue_ShouldReturnCorrectPermission(string value, EPermission expectedPermission)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().Be(expectedPermission);
    }

    [Theory]
    [InlineData("USERS:READ", EPermission.UsersRead)]
    [InlineData("SYSTEM:ADMIN", EPermission.SystemAdmin)]
    [InlineData("Providers:Approve", EPermission.ProvidersApprove)]
    public void FromValue_WithCaseInsensitiveValue_ShouldReturnCorrectPermission(string value, EPermission expectedPermission)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().Be(expectedPermission);
    }

    [Theory]
    [InlineData("invalid:permission")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("users:")]
    [InlineData(":read")]
    [InlineData("unknown:module")]
    public void FromValue_WithInvalidValue_ShouldReturnNull(string value)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.FromValue(value);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromValue_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        // (none)

        // Act
        var result = PermissionExtensions.FromValue(null!);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPermissionsByModule Tests

    [Theory]
    [InlineData("users")]
    [InlineData("system")]
    [InlineData("providers")]
    [InlineData("bookings")]
    [InlineData("payments")]
    [InlineData("communications")]
    [InlineData("ratings")]
    [InlineData("search")]
    [InlineData("documents")]
    [InlineData("service-catalogs")]
    [InlineData("locations")]
    [InlineData("reports")]
    [InlineData("admin")]
    public void GetPermissionsByModule_WithValidModule_ShouldReturnOnlyModulePermissions(string module)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(p => p.GetModule().Should().Be(module));
    }

    [Theory]
    [InlineData("USERS")]
    [InlineData("System")]
    [InlineData("PROVIDERS")]
    public void GetPermissionsByModule_WithCaseInsensitiveModule_ShouldReturnPermissions(string module)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("nonexistent")]
    [InlineData("")]
    [InlineData("unknown")]
    public void GetPermissionsByModule_WithInvalidModule_ShouldReturnEmptyList(string module)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(module).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPermissionsByModule_WithNullModule_ShouldReturnEmptyList()
    {
        // Arrange
        // (none)

        // Act
        var result = PermissionExtensions.GetPermissionsByModule(null!).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAllModules Tests

    [Fact]
    public void GetAllModules_ShouldReturnAllDistinctModules()
    {
        // Arrange
        // (none)

        // Act
        var result = PermissionExtensions.GetAllModules().ToList();

        // Assert
        result.Should().Contain("users");
        result.Should().Contain("system");
        result.Should().Contain("providers");
        result.Should().Contain("bookings");
        result.Should().Contain("payments");
        result.Should().Contain("communications");
        result.Should().Contain("ratings");
        result.Should().Contain("search");
        result.Should().Contain("documents");
        result.Should().Contain("service-catalogs");
        result.Should().Contain("locations");
        result.Should().Contain("reports");
        result.Should().Contain("admin");
        result.Should().NotContain("unknown");
        result.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetAllModules_ShouldReturnSortedModules()
    {
        // Arrange
        // (none)

        // Act
        var result = PermissionExtensions.GetAllModules().ToList();

        // Assert
        result.Should().BeInAscendingOrder();
    }

    #endregion

    #region IsAdminPermission Tests

    [Theory]
    [InlineData(EPermission.AdminSystem)]
    [InlineData(EPermission.AdminUsers)]
    [InlineData(EPermission.AdminReports)]
    public void IsAdminPermission_WithAdminModulePermission_ShouldReturnTrue(EPermission permission)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(EPermission.SystemAdmin)]
    [InlineData(EPermission.SystemRead)]
    [InlineData(EPermission.UsersRead)]
    [InlineData(EPermission.UsersCreate)]
    [InlineData(EPermission.ProvidersRead)]
    [InlineData(EPermission.ProvidersCreate)]
    [InlineData(EPermission.BookingsRead)]
    [InlineData(EPermission.PaymentsRead)]
    [InlineData(EPermission.CommunicationsRead)]
    [InlineData(EPermission.RatingsRead)]
    [InlineData(EPermission.SearchRead)]
    [InlineData(EPermission.DocumentsRead)]
    [InlineData(EPermission.ReportsView)]
    [InlineData(EPermission.ServiceCatalogsRead)]
    [InlineData(EPermission.LocationsRead)]
    public void IsAdminPermission_WithNonAdminPermission_ShouldReturnFalse(EPermission permission)
    {
        // Arrange
        // (parameterized)

        // Act
        var result = permission.IsAdminPermission();

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
