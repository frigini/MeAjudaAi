using MeAjudaAi.Shared.Authorization.Core;
using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.Shared.Authorization.Handlers;

/// <summary>
/// Requirement de autorização que especifica uma permissão necessária.
/// Usado internamente pelo sistema de políticas de autorização.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// A permissão necessária para acessar o recurso.
    /// </summary>
    public EPermission Permission { get; }

    /// <summary>
    /// O valor string da permissão (derivado do enum).
    /// </summary>
    public string PermissionValue => Permission.GetValue();

    /// <summary>
    /// Inicializa o requirement com a permissão requerida.
    /// </summary>
    /// <param name="permission">A permissão necessária</param>
    /// <exception cref="ArgumentException">Lançado quando permission é EPermission.None</exception>
    public PermissionRequirement(EPermission permission)
    {
        if (permission == EPermission.None)
            throw new ArgumentException("EPermission.None não é uma permissão válida para autorização", nameof(permission));

        Permission = permission;
    }
}
