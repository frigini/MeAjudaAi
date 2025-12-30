using System.Linq;
using System.Security.Claims;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization.Middleware;

/// <summary>
/// Middleware para otimização de permissões que evita consultas desnecessárias
/// e melhora a performance do sistema de autorização.
/// </summary>
public sealed class PermissionOptimizationMiddleware(
    RequestDelegate next,
    ILogger<PermissionOptimizationMiddleware> logger)
{

    // Endpoints que não precisam de verificação de permissões
    private static readonly HashSet<string> PublicEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        ApiEndpoints.System.Health,
        ApiEndpoints.System.HealthReady,
        ApiEndpoints.System.HealthLive,
        "/metrics",
        "/swagger",
        "/api/auth/login",
        "/api/auth/logout",
        "/api/auth/refresh",
        "/.well-known/openid-configuration"
    };

    // Métodos HTTP que geralmente não precisam de permissões complexas
    private static readonly HashSet<string> ReadOnlyMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip otimização para endpoints públicos
        if (IsPublicEndpoint(context.Request.Path))
        {
            await next(context);
            return;
        }

        // Skip se usuário não está autenticado
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // Aplica otimizações baseadas no contexto da requisição
        await ApplyPermissionOptimizationsAsync(context);

        await next(context);
    }

    /// <summary>
    /// Aplica otimizações específicas baseadas no contexto da requisição.
    /// </summary>
    private async Task ApplyPermissionOptimizationsAsync(HttpContext context)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Otimização 1: Cache de contexto da requisição
            await CacheRequestContextAsync(context);

            // Otimização 2: Pre-load de permissões para operações conhecidas
            await PreloadKnownPermissionsAsync(context);

            // Otimização 3: Bypass para operações de leitura simples
            ApplyReadOnlyOptimizations(context);

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 100) // Log apenas se demorar mais que 100ms
            {
                logger.LogWarning("Permission optimization took {ElapsedMs}ms for {Method} {Path}",
                    stopwatch.ElapsedMilliseconds, context.Request.Method, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during permission optimization for {Method} {Path}",
                context.Request.Method, context.Request.Path);
            // Não falha a requisição por causa de otimização
        }
    }

    /// <summary>
    /// Cacheia informações de contexto da requisição para evitar re-computação.
    /// </summary>
    private static async Task CacheRequestContextAsync(HttpContext context)
    {
        var userId = GetUserId(context.User);
        if (string.IsNullOrEmpty(userId))
            return;

        // Cacheia informações básicas do usuário no contexto da requisição
        context.Items["UserId"] = userId;
        context.Items["UserTenant"] = context.User.GetTenantId();
        context.Items["UserOrganization"] = context.User.GetOrganizationId();
        context.Items["IsSystemAdmin"] = context.User.IsSystemAdmin();

        // Cacheia timestamp para controle de cache
        context.Items["PermissionCacheTimestamp"] = DateTimeOffset.UtcNow;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Pre-carrega permissões conhecidas baseadas na rota da requisição.
    /// </summary>
    private async Task PreloadKnownPermissionsAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(path))
            return;

        var userId = context.Items["UserId"] as string;
        if (string.IsNullOrEmpty(userId))
            return;

        // Identifica permissões necessárias baseadas na rota
        var requiredPermissions = GetRequiredPermissionsForPath(path, context.Request.Method);

        if (requiredPermissions.Any())
        {
            // Armazena as permissões esperadas no contexto para otimização downstream
            context.Items["ExpectedPermissions"] = requiredPermissions;

            logger.LogDebug("Pre-identified {PermissionCount} required permissions for {Method} {Path}",
                requiredPermissions.Count, context.Request.Method, path);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica otimizações específicas para operações de leitura.
    /// </summary>
    private static void ApplyReadOnlyOptimizations(HttpContext context)
    {
        if (!ReadOnlyMethods.Contains(context.Request.Method))
            return;

        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(path))
            return;

        // Para operações de leitura em endpoints específicos, pode usar cache mais agressivo
        if (path.StartsWith("/api/v1/users/profile", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(ApiEndpoints.System.Health, StringComparison.OrdinalIgnoreCase))
        {
            context.Items["UseAggressivePermissionCache"] = true;
            context.Items["PermissionCacheDuration"] = TimeSpan.FromMinutes(30);
        }
        else if (path.StartsWith("/api/") && context.Request.Method == "GET")
        {
            // Catch-all para operações GET em qualquer versão da API - cache intermediário
            // Suporta múltiplas versões da API (v1, v2, etc.) para compatibilidade
            context.Items["UseAggressivePermissionCache"] = false;
            context.Items["PermissionCacheDuration"] = TimeSpan.FromMinutes(10);
        }
    }

    /// <summary>
    /// Identifica permissões necessárias baseadas na rota e método HTTP.
    /// </summary>
    private static List<EPermission> GetRequiredPermissionsForPath(string path, string method)
    {
        var permissions = new List<EPermission>();

        // Users module
        if (path.StartsWith("/api/v1/users"))
        {
            permissions.AddRange(method.ToUpperInvariant() switch
            {
                "GET" when path.Contains("/profile") => new[] { EPermission.UsersProfile },
                "GET" when path.Contains("/admin") => new[] { EPermission.AdminUsers, EPermission.UsersList },
                "GET" => new[] { EPermission.UsersRead },
                "POST" => new[] { EPermission.UsersCreate },
                "PUT" or "PATCH" => new[] { EPermission.UsersUpdate },
                "DELETE" => new[] { EPermission.UsersDelete, EPermission.AdminUsers },
                _ => Array.Empty<EPermission>()
            });
        }

        // Providers module
        else if (path.StartsWith("/api/v1/providers"))
        {
            permissions.AddRange(method.ToUpperInvariant() switch
            {
                "GET" => new[] { EPermission.ProvidersRead },
                "POST" => new[] { EPermission.ProvidersCreate },
                "PUT" or "PATCH" => new[] { EPermission.ProvidersUpdate },
                "DELETE" => new[] { EPermission.ProvidersDelete },
                _ => Array.Empty<EPermission>()
            });
        }

        // Orders module (futuro) - Aguardando implementação do módulo completo
        else if (path.StartsWith("/api/v1/orders"))
        {
            permissions.AddRange(method.ToUpperInvariant() switch
            {
                "GET" => new[] { EPermission.OrdersRead },
                "POST" => new[] { EPermission.OrdersCreate },
                "PUT" or "PATCH" => new[] { EPermission.OrdersUpdate },
                "DELETE" => new[] { EPermission.OrdersDelete },
                _ => Array.Empty<EPermission>()
            });
        }

        // Reports module (futuro) - Aguardando implementação do módulo completo
        else if (path.StartsWith("/api/v1/reports"))
        {
            permissions.AddRange(method.ToUpperInvariant() switch
            {
                "GET" when path.Contains("/export") => new[] { EPermission.ReportsExport },
                "GET" => new[] { EPermission.ReportsView },
                "POST" => new[] { EPermission.ReportsCreate },
                _ => Array.Empty<EPermission>()
            });
        }

        // Admin endpoints - Verifica /api/v1/admin ou qualquer segmento "admin" no path
        else if (path.StartsWith("/api/v1/admin") || IsAdminPath(path))
        {
            permissions.Add(EPermission.AdminSystem);
        }

        return permissions;
    }

    /// <summary>
    /// Verifica se o path contém "admin" como um segmento distinto.
    /// </summary>
    private static bool IsAdminPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(segment =>
            string.Equals(segment, "admin", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica se o endpoint é público e não precisa de autenticação.
    /// </summary>
    private static bool IsPublicEndpoint(PathString path)
    {
        var pathValue = path.Value;
        if (string.IsNullOrEmpty(pathValue))
            return false;

        return PublicEndpoints.Any(endpoint =>
            pathValue.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extrai o ID do usuário dos claims.
    /// </summary>
    private static string? GetUserId(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               principal.FindFirst("sub")?.Value ??
               principal.FindFirst("id")?.Value;
    }
}

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
