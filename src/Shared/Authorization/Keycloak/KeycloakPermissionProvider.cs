using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization.Keycloak;

/// <summary>
/// Adapter que implementa IPermissionProvider usando o KeycloakPermissionResolver.
/// Permite que o PermissionService obtenha permissões resolvidas do Keycloak.
/// </summary>
public sealed class KeycloakPermissionProvider(
    IKeycloakPermissionResolver keycloakResolver,
    ILogger<KeycloakPermissionProvider> logger) : IPermissionProvider
{
    /// <inheritdoc/>
    public string ModuleName => ModuleNames.Users;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("GetUserPermissionsAsync called with empty userId");
            return Array.Empty<EPermission>();
        }

        try
        {
            var permissions = await keycloakResolver.ResolvePermissionsAsync(userId, cancellationToken);
            
            logger.LogDebug(
                "Resolved {PermissionCount} permissions from Keycloak for user {UserId}",
                permissions.Count, 
                MaskUserId(userId));
            
            return permissions;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to resolve permissions from Keycloak for user {UserId}",
                MaskUserId(userId));
            return Array.Empty<EPermission>();
        }
    }

    /// <summary>
    /// Mascara o ID do usuário para logging (evita exposição de PII).
    /// </summary>
    private static string MaskUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "[EMPTY]";

        if (userId.Length <= 6)
            return $"{userId[0]}***{userId[^1]}";

        return $"{userId[..3]}***{userId[^3..]}";
    }
}
