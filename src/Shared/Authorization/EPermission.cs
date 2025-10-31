using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Enum base que define todas as permissões do sistema de forma type-safe.
/// Cada módulo pode estender suas próprias permissões através de convenções.
/// Nomenclatura: EPermission (prefixo "E" para indicar Enum, plural para evitar conflito com palavra "Permission").
/// </summary>
public enum EPermission
{
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

    // ===== PROVIDERS MODULE (futuro) =====
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

    // ===== ORDERS MODULE (futuro) =====
    [Display(Name = "orders:read")]
    OrdersRead,

    [Display(Name = "orders:create")]
    OrdersCreate,

    [Display(Name = "orders:update")]
    OrdersUpdate,

    [Display(Name = "orders:cancel")]
    OrdersCancel,

    [Display(Name = "orders:list")]
    OrdersList,

    [Display(Name = "orders:fulfill")]
    OrdersFulfill,

    // ===== ORDERS MODULE - Delete =====
    [Display(Name = "orders:delete")]
    OrdersDelete,

    // ===== REPORTS MODULE (futuro) =====
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
    AdminReports
}
