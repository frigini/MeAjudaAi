using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Implementação modular do serviço de permissões que utiliza roles do Keycloak
/// e cache distribuído para otimizar performance. Suporta extensão por módulos.
/// </summary>
public sealed class PermissionService(
    ICacheService cacheService,
    IServiceProvider serviceProvider,
    ILogger<PermissionService> logger,
    IPermissionMetricsService metrics) : IPermissionService
{

    // Cache key patterns
    private const string UserPermissionsCacheKey = "user_permissions_{0}";
    private const string UserModulePermissionsCacheKey = "user_permissions_{0}_module_{1}";

    // Cache configuration
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = CacheExpiration,
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("GetUserPermissionsAsync called with empty userId");
            return Array.Empty<EPermission>();
        }

        using var timer = metrics.MeasurePermissionResolution(userId);

        var cacheKey = string.Format(UserPermissionsCacheKey, userId);
        var tags = new[] { "permissions", $"user:{userId}" };

        bool cacheHit = false;
        using var cacheTimer = metrics.MeasureCacheOperation("get_user_permissions", cacheHit);

        var result = await cacheService.GetOrCreateAsync(
            cacheKey,
            async _ =>
            {
                cacheHit = false; // Cache miss
                return await ResolveUserPermissionsAsync(userId, cancellationToken);
            },
            CacheExpiration,
            CacheOptions,
            tags,
            cancellationToken);

        if (result.Any())
        {
            cacheHit = true; // Had cached result
        }

        return result;
    }

    public async Task<bool> HasPermissionAsync(string userId, EPermission permission, CancellationToken cancellationToken = default)
    {
        using var timer = metrics.MeasurePermissionCheck(userId, permission, false); // Will update with actual result

        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var hasPermission = permissions.Contains(permission);

        if (!hasPermission)
        {
            metrics.RecordAuthorizationFailure(userId, permission, "Permission not granted");
        }

        return hasPermission;
    }

    public async Task<bool> HasPermissionsAsync(string userId, IEnumerable<EPermission> permissions, bool requireAll = true, CancellationToken cancellationToken = default)
    {
        if (!permissions.Any())
        {
            return true; // Vacuous truth - no permissions to check
        }

        using var timer = metrics.MeasureMultiplePermissionCheck(userId, permissions, requireAll);

        var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var userPermissionSet = userPermissions.ToHashSet();

        bool result = requireAll
            ? permissions.All(userPermissionSet.Contains)
            : permissions.Any(userPermissionSet.Contains);

        if (!result)
        {
            var missingPermissions = permissions.Where(p => !userPermissionSet.Contains(p));
            var reason = requireAll
                ? $"Missing required permissions: {string.Join(", ", missingPermissions)}"
                : $"None of the required permissions found: {string.Join(", ", permissions)}";

            metrics.RecordAuthorizationFailure(userId, permissions.First(), reason);
        }

        return result;
    }

    public async Task<IReadOnlyList<EPermission>> GetUserPermissionsByModuleAsync(string userId, string moduleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogWarning("GetUserPermissionsByModuleAsync called with empty userId");
            return Array.Empty<EPermission>();
        }

        if (string.IsNullOrWhiteSpace(moduleName))
        {
            logger.LogWarning("GetUserPermissionsByModuleAsync called with empty module name");
            return Array.Empty<EPermission>();
        }

        using var timer = metrics.MeasureModulePermissionResolution(userId, moduleName);

        var cacheKey = string.Format(UserModulePermissionsCacheKey, userId, moduleName);
        var tags = new[] { "permissions", $"user:{userId}", $"module:{moduleName}" };

        var result = await cacheService.GetOrCreateAsync(
            cacheKey,
            async _ => await ResolveUserModulePermissionsAsync(userId, moduleName, cancellationToken),
            CacheExpiration,
            CacheOptions,
            tags,
            cancellationToken);

        return result;
    }

    public async Task InvalidateUserPermissionsCacheAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        // Clear all user permission caches
        var userCacheKey = string.Format(UserPermissionsCacheKey, userId);
        await cacheService.RemoveByTagAsync($"user:{userId}", cancellationToken);

        logger.LogInformation("Invalidated permission cache for user {UserId}", userId);
    }

    // Private implementation methods
    private async Task<IReadOnlyList<EPermission>> ResolveUserPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        var permissions = new List<EPermission>();

        // Get all permission providers from DI
        var providers = serviceProvider.GetServices<IPermissionProvider>();

        foreach (var provider in providers)
        {
            try
            {
                var modulePermissions = await provider.GetUserPermissionsAsync(userId, cancellationToken);
                permissions.AddRange(modulePermissions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Permission provider {ProviderType} failed for user {UserId}",
                    provider.GetType().Name, userId);
            }
        }

        // Remove duplicates and return
        return permissions.Distinct().ToArray();
    }

    private async Task<IReadOnlyList<EPermission>> ResolveUserModulePermissionsAsync(string userId, string moduleName, CancellationToken cancellationToken)
    {
        var permissions = new List<EPermission>();

        // Get module-specific permission providers
        var providers = serviceProvider.GetServices<IPermissionProvider>()
            .Where(p => p.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

        foreach (var provider in providers)
        {
            try
            {
                var modulePermissions = await provider.GetUserPermissionsAsync(userId, cancellationToken);
                permissions.AddRange(modulePermissions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Module permission provider {ProviderType} failed for user {UserId} in module {ModuleName}",
                    provider.GetType().Name, userId, moduleName);
            }
        }

        return permissions.Distinct().ToArray();
    }
}
