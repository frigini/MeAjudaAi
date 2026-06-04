using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

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
    /// <remarks>
    /// Retorna "*" (wildcard) para indicar que este provider fornece permissões
    /// para todos os módulos, não apenas Users. Isso permite que permissões
    /// do Keycloak sejam aplicadas cross-module.
    /// </remarks>
    public string ModuleName => "*";

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
            var permissions = await keycloakResolver.ResolvePermissionsAsync(userId, cancellationToken)
                ?? Array.Empty<EPermission>();

            logger.LogDebug(
                "Resolved {PermissionCount} permissions from Keycloak for user {UserId}",
                permissions.Count,
                MaskUserId(userId));

            return permissions;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Network error while resolving permissions from Keycloak for user {UserId}",
                MaskUserId(userId));
            return Array.Empty<EPermission>();
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Deserialization error while resolving permissions from Keycloak for user {UserId}",
                MaskUserId(userId));
            return Array.Empty<EPermission>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while resolving permissions from Keycloak for user {UserId}",
                MaskUserId(userId));
            throw;
        }
    }

    /// <summary>
    /// Mascara o ID do usuário para logging (evita exposição de PII).
    /// </summary>
    private static string MaskUserId(string userId) => PiiMaskingHelper.MaskUserId(userId);
}
