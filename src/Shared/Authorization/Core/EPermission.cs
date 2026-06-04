using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Shared.Authorization.Core;

/// <summary>
/// Enum base que define todas as permissões do sistema de forma type-safe.
/// Cada módulo pode estender suas próprias permissões através de convenções.
/// Nomenclatura: EPermission (prefixo "E" para indicar Enum, plural para evitar conflito com palavra "Permission").
/// </summary>
public enum EPermission
{
    /// <summary>
    /// Permissão não definida
    /// </summary>
    [Display(Name = "system:none")]
    None = 0,

    // ===== SISTEMA - GLOBAL =====
    [Display(Name = "system:read")]
    SystemRead,

    [Display(Name = "system:write")]
    SystemWrite,

    [Display(Name = "system:admin")]
    SystemAdmin,

    // ===== USERS MODULE =====
    [Display(Name = "users:read")]
    UsersRead,

    [Display(Name = "users:create")]
    UsersCreate,

    [Display(Name = "users:update")]
    UsersUpdate,

    [Display(Name = "users:delete")]
    UsersDelete,

    [Display(Name = "users:list")]
    UsersList,

    [Display(Name = "users:profile")]
    UsersProfile,

    [Display(Name = "users:register")]
    UsersRegister,

    // ===== PROVIDERS MODULE =====
    [Display(Name = "providers:read")]
    ProvidersRead,

    [Display(Name = "providers:create")]
    ProvidersCreate,

    [Display(Name = "providers:update")]
    ProvidersUpdate,

    [Display(Name = "providers:delete")]
    ProvidersDelete,

    [Display(Name = "providers:list")]
    ProvidersList,

    [Display(Name = "providers:approve")]
    ProvidersApprove,

    [Display(Name = "providers:register")]
    ProvidersRegister,

    [Display(Name = "providers:upload-documents")]
    ProvidersUploadDocuments,

    [Display(Name = "providers:manage-tier")]
    ProvidersManageTier,

    // ===== SERVICE CATALOGS MODULE =====
    [Display(Name = "service-catalogs:read")]
    ServiceCatalogsRead,

    [Display(Name = "service-catalogs:manage")]
    ServiceCatalogsManage,

    // ===== LOCATIONS MODULE =====
    [Display(Name = "locations:read")]
    LocationsRead,

    [Display(Name = "locations:manage")]
    LocationsManage,

    // ===== BOOKINGS MODULE =====
    [Display(Name = "bookings:read")]
    BookingsRead,

    [Display(Name = "bookings:create")]
    BookingsCreate,

    [Display(Name = "bookings:update")]
    BookingsUpdate,

    [Display(Name = "bookings:cancel")]
    BookingsCancel,

    [Display(Name = "bookings:list")]
    BookingsList,

    [Display(Name = "bookings:manage")]
    BookingsManage,

    // ===== PAYMENTS MODULE =====
    [Display(Name = "payments:read")]
    PaymentsRead,

    [Display(Name = "payments:manage")]
    PaymentsManage,

    [Display(Name = "payments:checkout")]
    PaymentsCheckout,

    // ===== COMMUNICATIONS MODULE =====
    [Display(Name = "communications:read")]
    CommunicationsRead,

    [Display(Name = "communications:send")]
    CommunicationsSend,

    [Display(Name = "communications:manage")]
    CommunicationsManage,

    // ===== RATINGS MODULE =====
    [Display(Name = "ratings:read")]
    RatingsRead,

    [Display(Name = "ratings:create")]
    RatingsCreate,

    [Display(Name = "ratings:moderate")]
    RatingsModerate,

    // ===== SEARCH PROVIDERS MODULE =====
    [Display(Name = "search:read")]
    SearchRead,

    [Display(Name = "search:manage")]
    SearchManage,

    // ===== DOCUMENTS MODULE =====
    [Display(Name = "documents:read")]
    DocumentsRead,

    [Display(Name = "documents:upload")]
    DocumentsUpload,

    [Display(Name = "documents:verify")]
    DocumentsVerify,

    [Display(Name = "documents:delete")]
    DocumentsDelete,

    // ===== ANALYTICS & REPORTS (PLANNED) =====
    [Display(Name = "reports:view")]
    ReportsView,

    [Display(Name = "reports:export")]
    ReportsExport,

    [Display(Name = "reports:create")]
    ReportsCreate,

    [Display(Name = "reports:admin")]
    ReportsAdmin,

    // ===== ADMIN PERMISSIONS =====
    [Display(Name = "admin:system")]
    AdminSystem,

    [Display(Name = "admin:users")]
    AdminUsers,

    [Display(Name = "admin:reports")]
    AdminReports,
}
