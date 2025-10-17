namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Serviço responsável por gerenciar permissões de usuários.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Obtém todas as permissões de um usuário específico.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões do usuário</returns>
    Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um usuário possui uma permissão específica.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="permission">Permissão a verificar</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o usuário possui a permissão</returns>
    Task<bool> HasPermissionAsync(string userId, EPermission permission, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um usuário possui múltiplas permissões.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="permissions">Permissões a verificar</param>
    /// <param name="requireAll">Se true, requer todas as permissões; se false, requer ao menos uma</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o usuário atende aos critérios</returns>
    Task<bool> HasPermissionsAsync(string userId, IEnumerable<EPermission> permissions, bool requireAll = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém permissões de um usuário por módulo específico.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="module">Nome do módulo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões do módulo</returns>
    Task<IReadOnlyList<EPermission>> GetUserPermissionsByModuleAsync(string userId, string module, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalida o cache de permissões de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task InvalidateUserPermissionsCacheAsync(string userId, CancellationToken cancellationToken = default);
}