using MeAjudaAi.Shared.Authorization.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Authorization.Core;

/// <summary>
/// Compatibility layer for Permission type. 
/// This provides backward compatibility while migrating to EPermission enum.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Permission
{
    // System permissions
    public static EPermission SystemRead => EPermission.SystemRead;
    public static EPermission SystemWrite => EPermission.SystemWrite;
    public static EPermission SystemAdmin => EPermission.SystemAdmin;

    // Users permissions
    public static EPermission UsersRead => EPermission.UsersRead;
    public static EPermission UsersCreate => EPermission.UsersCreate;
    public static EPermission UsersUpdate => EPermission.UsersUpdate;
    public static EPermission UsersDelete => EPermission.UsersDelete;
    public static EPermission UsersList => EPermission.UsersList;
    public static EPermission UsersProfile => EPermission.UsersProfile;
    public static EPermission UsersRegister => EPermission.UsersRegister;

    // Providers permissions
    public static EPermission ProvidersRead => EPermission.ProvidersRead;
    public static EPermission ProvidersCreate => EPermission.ProvidersCreate;
    public static EPermission ProvidersUpdate => EPermission.ProvidersUpdate;
    public static EPermission ProvidersDelete => EPermission.ProvidersDelete;
    public static EPermission ProvidersList => EPermission.ProvidersList;
    public static EPermission ProvidersApprove => EPermission.ProvidersApprove;
    public static EPermission ProvidersRegister => EPermission.ProvidersRegister;
    public static EPermission ProvidersUploadDocuments => EPermission.ProvidersUploadDocuments;
    public static EPermission ProvidersManageTier => EPermission.ProvidersManageTier;

    // Service Catalogs permissions
    public static EPermission ServiceCatalogsRead => EPermission.ServiceCatalogsRead;
    public static EPermission ServiceCatalogsManage => EPermission.ServiceCatalogsManage;

    // Locations permissions
    public static EPermission LocationsRead => EPermission.LocationsRead;
    public static EPermission LocationsManage => EPermission.LocationsManage;

    // Bookings permissions
    public static EPermission BookingsRead => EPermission.BookingsRead;
    public static EPermission BookingsCreate => EPermission.BookingsCreate;
    public static EPermission BookingsUpdate => EPermission.BookingsUpdate;
    public static EPermission BookingsCancel => EPermission.BookingsCancel;
    public static EPermission BookingsList => EPermission.BookingsList;
    public static EPermission BookingsManage => EPermission.BookingsManage;

    // Payments permissions
    public static EPermission PaymentsRead => EPermission.PaymentsRead;
    public static EPermission PaymentsManage => EPermission.PaymentsManage;
    public static EPermission PaymentsCheckout => EPermission.PaymentsCheckout;

    // Communications permissions
    public static EPermission CommunicationsRead => EPermission.CommunicationsRead;
    public static EPermission CommunicationsSend => EPermission.CommunicationsSend;
    public static EPermission CommunicationsManage => EPermission.CommunicationsManage;

    // Ratings permissions
    public static EPermission RatingsRead => EPermission.RatingsRead;
    public static EPermission RatingsCreate => EPermission.RatingsCreate;
    public static EPermission RatingsModerate => EPermission.RatingsModerate;

    // Search permissions
    public static EPermission SearchRead => EPermission.SearchRead;
    public static EPermission SearchManage => EPermission.SearchManage;

    // Documents permissions
    public static EPermission DocumentsRead => EPermission.DocumentsRead;
    public static EPermission DocumentsUpload => EPermission.DocumentsUpload;
    public static EPermission DocumentsVerify => EPermission.DocumentsVerify;
    public static EPermission DocumentsDelete => EPermission.DocumentsDelete;

    // Reports permissions
    public static EPermission ReportsView => EPermission.ReportsView;
    public static EPermission ReportsExport => EPermission.ReportsExport;
    public static EPermission ReportsCreate => EPermission.ReportsCreate;
    public static EPermission ReportsAdmin => EPermission.ReportsAdmin;

    // Admin permissions
    public static EPermission AdminSystem => EPermission.AdminSystem;
    public static EPermission AdminUsers => EPermission.AdminUsers;
    public static EPermission AdminReports => EPermission.AdminReports;
}
