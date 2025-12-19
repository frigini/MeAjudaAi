using MeAjudaAi.Shared.Authorization.ValueObjects;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Shared.Authorization.Core;

/// <summary>
/// Interface para resolvers de permissões específicos de cada módulo.
/// Cada módulo pode implementar este contrato para fornecer suas próprias
/// regras de resolução de permissões baseadas em roles, contexto, etc.
/// </summary>
public interface IModulePermissionResolver
{
    /// <summary>
    /// Nome do módulo que este resolver atende.
    /// Usado para identificação e debug.
    /// Use as constantes definidas em <see cref="ModuleNames"/> para consistência.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Resolve as permissões de um usuário dentro do contexto deste módulo.
    /// </summary>
    /// <param name="userId">ID do usuário usando value object para type safety</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões resolvidas para o usuário neste módulo</returns>
    Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o resolver pode lidar com uma permissão específica.
    /// Útil para casos onde múltiplos módulos podem ter permissões sobrepostas.
    /// </summary>
    /// <param name="permission">Permissão a ser verificada</param>
    /// <returns>True se o resolver pode lidar com esta permissão</returns>
    bool CanResolve(EPermission permission);
}
