using MeAjudaAi.Shared.Authorization.Core;

namespace MeAjudaAi.Shared.Authorization.Keycloak;

/// <summary>
/// Interface específica para o resolver de permissões do Keycloak.
/// Estende IModulePermissionResolver com funcionalidades específicas do Keycloak.
/// </summary>
public interface IKeycloakPermissionResolver : IModulePermissionResolver
{
    /// <summary>
    /// Obtém as roles do usuário diretamente do Keycloak.
    /// </summary>
    /// <param name="userId">ID do usuário como string (para compatibilidade com Keycloak)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de roles do Keycloak</returns>
    Task<IReadOnlyList<string>> GetUserRolesFromKeycloakAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mapeia uma role do Keycloak para permissões do sistema.
    /// </summary>
    /// <param name="keycloakRole">Role do Keycloak</param>
    /// <returns>Permissões correspondentes</returns>
    IEnumerable<EPermission> MapKeycloakRoleToPermissions(string keycloakRole);
}
