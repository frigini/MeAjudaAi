using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Modules.Users.API.Authorization;

/// <summary>
/// Define as permissões específicas do módulo Users de forma centralizada.
/// Facilita manutenção e documentação das permissões por módulo.
/// </summary>
public static class UsersPermissions
{
    /// <summary>
    /// Permissões básicas de leitura de usuários.
    /// </summary>
    internal static class Read
    {
        public const EPermission OwnProfile = EPermission.UsersProfile;
        public const EPermission UsersList = EPermission.UsersList;
        public const EPermission UserDetails = EPermission.UsersRead;
    }

    /// <summary>
    /// Permissões de escrita/modificação de usuários.
    /// </summary>
    internal static class Write
    {
        public const EPermission CreateUser = EPermission.UsersCreate;
        public const EPermission UpdateUser = EPermission.UsersUpdate;
        public const EPermission DeleteUser = EPermission.UsersDelete;
    }

    /// <summary>
    /// Permissões administrativas do módulo de usuários.
    /// </summary>
    internal static class Admin
    {
        public const EPermission SystemAdmin = EPermission.SystemAdmin;
        public const EPermission ManageAllUsers = EPermission.UsersList;
    }

    /// <summary>
    /// Grupos de permissões comuns para facilitar uso em policies.
    /// </summary>
    internal static class Groups
    {
        /// <summary>
        /// Permissões de usuário básico (próprio perfil).
        /// </summary>
        public static readonly EPermission[] BasicUser =
        {
            EPermission.UsersProfile,
            EPermission.UsersRead
        };

        /// <summary>
        /// Permissões de administrador de usuários.
        /// </summary>
        public static readonly EPermission[] UserAdmin =
        {
            EPermission.UsersList,
            EPermission.UsersRead,
            EPermission.UsersCreate,
            EPermission.UsersUpdate,
            EPermission.UsersDelete
        };

        /// <summary>
        /// Permissões de administrador de sistema.
        /// </summary>
        public static readonly EPermission[] SystemAdmin =
        {
            EPermission.SystemAdmin,
            EPermission.SystemRead,
            EPermission.SystemWrite,
            EPermission.UsersList,
            EPermission.UsersRead,
            EPermission.UsersCreate,
            EPermission.UsersUpdate,
            EPermission.UsersDelete
        };
    }
}
