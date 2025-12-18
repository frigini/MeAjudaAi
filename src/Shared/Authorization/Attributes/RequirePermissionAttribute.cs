using MeAjudaAi.Shared.Authorization.Core;
using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.Shared.Authorization.Attributes;

/// <summary>
/// Atributo de autorização que requer uma permissão específica de forma type-safe.
/// Substitui o uso de magic strings por enums.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationRequirement
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
    /// Inicializa o atributo com a permissão requerida.
    /// </summary>
    /// <param name="permission">A permissão necessária</param>
    /// <exception cref="ArgumentException">Lançado quando permission é EPermission.None</exception>
    public RequirePermissionAttribute(EPermission permission)
    {
        if (permission == EPermission.None)
            throw new ArgumentException("EPermission.None não é uma permissão válida para autorização", nameof(permission));

        Permission = permission;
        Policy = $"RequirePermission:{permission.GetValue()}";
    }
}
