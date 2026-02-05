using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para IHttpContextAccessor e HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Obtém a identidade do usuário para fins de auditoria (UpdatedBy, CreatedBy).
    /// Tenta resolver na ordem: Email -> PreferredUsername -> NameIdentifier -> "system".
    /// </summary>
    public static string GetAuditIdentity(this IHttpContextAccessor? httpContextAccessor)
    {
        var principal = httpContextAccessor?.HttpContext?.User;
        if (principal == null)
            return "system";

        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
            return email;

        var preferredUsername = principal.FindFirstValue(AuthConstants.Claims.PreferredUsername);
        if (!string.IsNullOrWhiteSpace(preferredUsername))
            return preferredUsername;

        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(subject))
            return subject;

        return "system";
    }
}
