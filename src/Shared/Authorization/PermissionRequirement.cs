using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.Shared.Authorization;

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
    public PermissionRequirement(EPermission permission)
    {
        Permission = permission;
    }
}