using MeAjudaAi.Shared.Utilities.Constants;
using System.Security.Claims;

namespace MeAjudaAi.Shared.Authorization.Extensions;

/// <summary>
/// Extensions para ClaimsPrincipal para facilitar acesso aos claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Obtém o ID do tenant do usuário.
    /// </summary>
    /// <param name="principal">O principal</param>
    /// <returns>O ID do tenant ou null se não encontrado</returns>
    public static string? GetTenantId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(AuthConstants.Claims.TenantId)?.Value;
    }

    /// <summary>
    /// Obtém o ID da organização do usuário.
    /// </summary>
    /// <param name="principal">O principal</param>
    /// <returns>O ID da organização ou null se não encontrado</returns>
    public static string? GetOrganizationId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(AuthConstants.Claims.Organization)?.Value;
    }

    /// <summary>
    /// Obtém o ID do usuário.
    /// </summary>
    /// <param name="principal">O principal</param>
    /// <returns>O ID do usuário ou null se não encontrado</returns>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(AuthConstants.Claims.Subject)?.Value;
    }

    /// <summary>
    /// Obtém o email do usuário.
    /// </summary>
    /// <param name="principal">O principal</param>
    /// <returns>O email do usuário ou null se não encontrado</returns>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.FindFirst(AuthConstants.Claims.Email)?.Value;
    }
}
