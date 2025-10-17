namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Interface para provedores de permissões modulares.
/// Cada módulo pode implementar seu próprio provedor de permissões.
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Nome do módulo responsável por este provedor
    /// </summary>
    string ModuleName { get; }
    
    /// <summary>
    /// Obtém as permissões de um usuário específico para este módulo
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões do usuário para este módulo</returns>
    Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
}