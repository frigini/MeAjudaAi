using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.Shared.Authorization;

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
    public RequirePermissionAttribute(EPermission permission)
    {
        Permission = permission;
        Policy = $"RequirePermission:{permission.GetValue()}";
    }
}
