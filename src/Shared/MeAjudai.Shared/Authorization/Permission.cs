using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Compatibility layer for Permission type. 
/// This provides backward compatibility while migrating to EPermissions enum.
/// </summary>
public static class Permission
{
    // System permissions
    public static EPermission SystemRead => EPermission.SystemRead;
    public static EPermission SystemWrite => EPermission.SystemWrite;
    
    // Users permissions
    public static EPermission UsersRead => EPermission.UsersRead;
    public static EPermission UsersCreate => EPermission.UsersCreate;
    public static EPermission UsersUpdate => EPermission.UsersUpdate;
    public static EPermission UsersDelete => EPermission.UsersDelete;
    public static EPermission UsersList => EPermission.UsersList;
    public static EPermission UsersProfile => EPermission.UsersProfile;
    
    // Providers permissions
    public static EPermission ProvidersRead => EPermission.ProvidersRead;
    public static EPermission ProvidersCreate => EPermission.ProvidersCreate;
    public static EPermission ProvidersUpdate => EPermission.ProvidersUpdate;
    public static EPermission ProvidersDelete => EPermission.ProvidersDelete;
    public static EPermission ProvidersList => EPermission.ProvidersList;
    public static EPermission ProvidersApprove => EPermission.ProvidersApprove;
    
    // Orders permissions  
    public static EPermission OrdersRead => EPermission.OrdersRead;
    public static EPermission OrdersCreate => EPermission.OrdersCreate;
    public static EPermission OrdersUpdate => EPermission.OrdersUpdate;
    public static EPermission OrdersDelete => EPermission.OrdersDelete;
    public static EPermission OrdersList => EPermission.OrdersList;
    public static EPermission OrdersCancel => EPermission.OrdersCancel;
    public static EPermission OrdersFulfill => EPermission.OrdersFulfill;
    
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