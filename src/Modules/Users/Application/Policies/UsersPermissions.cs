using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Modules.Users.Application.Policies;

/// <summary>
/// Define arrays estáticos de permissões para diferentes roles no módulo Users.
/// </summary>
public static class UsersPermissions
{
    /// <summary>
    /// Permissões básicas para usuários normais.
    /// </summary>
    public static readonly EPermission[] BasicUser = [
        EPermission.UsersRead
    ];

    /// <summary>
    /// Permissões para administradores de usuários.
    /// </summary>
    public static readonly EPermission[] UserAdmin = [
        EPermission.UsersRead,
        EPermission.UsersUpdate
    ];

    /// <summary>
    /// Permissões para administradores do sistema.
    /// </summary>
    public static readonly EPermission[] SystemAdmin = [
        EPermission.UsersRead,
        EPermission.UsersUpdate,
        EPermission.UsersDelete,
        EPermission.AdminUsers
    ];
}