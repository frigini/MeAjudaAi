using System.Security.Claims;
using MeAjudaAi.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Transforma claims do usuário adicionando permissões baseadas em roles.
/// Executa automaticamente a cada requisição autenticada.
/// </summary>
public sealed class PermissionClaimsTransformation(
    IPermissionService permissionService,
    ILogger<PermissionClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Só processa usuários autenticados
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        // Verifica se já possui claims de permissão (evita processamento duplo)
        if (principal.HasClaim(CustomClaimTypes.Permission, "*"))
            return principal;

        var userId = GetUserId(principal);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Unable to extract user ID from authenticated principal");
            return principal;
        }

        try
        {
            // Obtém permissões do usuário
            var permissions = await permissionService.GetUserPermissionsAsync(userId);

            if (!permissions.Any())
            {
                logger.LogDebug("No permissions found for user {UserId}", userId);
                return principal;
            }

            // Cria nova identidade com as permissões
            var claimsIdentity = new ClaimsIdentity(principal.Identity);

            // Adiciona claims de permissão
            foreach (var permission in permissions)
            {
                claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Permission, permission.GetValue()));
                claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Module, permission.GetModule()));
            }

            // Adiciona flag indicando que permissões foram processadas
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Permission, "*"));

            // Adiciona flag de admin se aplicável
            if (permissions.Any(p => p.IsAdminPermission()))
            {
                claimsIdentity.AddClaim(new Claim(CustomClaimTypes.IsSystemAdmin, "true"));
            }

            logger.LogDebug("Added {PermissionCount} permission claims for user {UserId}",
                permissions.Count, userId);

            return new ClaimsPrincipal(claimsIdentity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to transform claims for user {UserId}", userId);
            return principal;
        }
    }

    /// <summary>
    /// Extrai o ID do usuário dos claims.
    /// </summary>
    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               principal.FindFirst(AuthConstants.Claims.Subject)?.Value ??
               principal.FindFirst("id")?.Value;
    }
}
