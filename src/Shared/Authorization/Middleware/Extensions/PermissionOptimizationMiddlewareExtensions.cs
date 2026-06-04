using MeAjudaAi.Shared.Authorization.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Authorization.Middleware.Extensions;

/// <summary>
/// Extensões para facilitar o uso do middleware de otimização de permissões.
/// </summary>
public static class PermissionOptimizationMiddlewareExtensions
{
    /// <summary>
    /// Adiciona o middleware de otimização de permissões ao pipeline.
    /// </summary>
    public static IApplicationBuilder UsePermissionOptimization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PermissionOptimizationMiddleware>();
    }

    /// <summary>
    /// Obtém as permissões esperadas para a requisição atual (se disponíveis).
    /// </summary>
    public static IEnumerable<EPermission> GetExpectedPermissions(this HttpContext context)
    {
        if (context.Items.TryGetValue("ExpectedPermissions", out var permissions))
        {
            return permissions as IEnumerable<EPermission> ?? Enumerable.Empty<EPermission>();
        }

        return Enumerable.Empty<EPermission>();
    }

    /// <summary>
    /// Verifica se deve usar cache agressivo de permissões para esta requisição.
    /// </summary>
    public static bool ShouldUseAggressivePermissionCache(this HttpContext context)
    {
        return context.Items.TryGetValue("UseAggressivePermissionCache", out var useCache) &&
               useCache is bool useCacheBool && useCacheBool;
    }

    /// <summary>
    /// Obtém a duração recomendada do cache de permissões para esta requisição.
    /// </summary>
    public static TimeSpan GetRecommendedPermissionCacheDuration(this HttpContext context)
    {
        if (context.Items.TryGetValue("PermissionCacheDuration", out var duration) &&
            duration is TimeSpan durationTimeSpan)
        {
            return durationTimeSpan;
        }

        return TimeSpan.FromMinutes(15); // Default
    }
}
