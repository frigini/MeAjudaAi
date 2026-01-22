namespace MeAjudaAi.Web.Admin.Services.Interfaces;

/// <summary>
/// Serviço para verificar permissões do usuário atual.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Verifica se o usuário atual tem a permissão especificada pela política.
    /// </summary>
    /// <param name="policyName">Nome da política a verificar</param>
    /// <returns>True se o usuário tem permissão, False caso contrário</returns>
    Task<bool> HasPermissionAsync(string policyName);

    /// <summary>
    /// Verifica se o usuário atual possui uma ou mais roles específicas.
    /// </summary>
    /// <param name="roles">Roles a verificar</param>
    /// <returns>True se o usuário possui pelo menos uma das roles</returns>
    Task<bool> HasAnyRoleAsync(params string[] roles);

    /// <summary>
    /// Verifica se o usuário atual possui todas as roles especificadas.
    /// </summary>
    /// <param name="roles">Roles a verificar</param>
    /// <returns>True se o usuário possui todas as roles</returns>
    Task<bool> HasAllRolesAsync(params string[] roles);

    /// <summary>
    /// Obtém todas as roles do usuário atual.
    /// </summary>
    /// <returns>Lista de roles do usuário</returns>
    Task<IEnumerable<string>> GetUserRolesAsync();

    /// <summary>
    /// Verifica se o usuário atual é administrador.
    /// </summary>
    /// <returns>True se o usuário é admin</returns>
    Task<bool> IsAdminAsync();
}
