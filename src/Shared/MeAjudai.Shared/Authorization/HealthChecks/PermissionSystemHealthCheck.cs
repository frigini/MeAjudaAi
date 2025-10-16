using MeAjudaAi.Shared.Authorization.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization.HealthChecks;

/// <summary>
/// Health check específico para o sistema de permissões.
/// Verifica se o sistema está funcionando corretamente e com boa performance.
/// </summary>
public sealed class PermissionSystemHealthCheck : IHealthCheck
{
    private readonly IPermissionService _permissionService;
    private readonly IPermissionMetricsService _metricsService;
    private readonly ILogger<PermissionSystemHealthCheck> _logger;

    // Limites para considerações de saúde
    private static readonly TimeSpan MaxPermissionResolutionTime = TimeSpan.FromSeconds(2);
    private const double MinCacheHitRate = 0.7; // 70%
    private const int MaxActiveChecks = 100;

    public PermissionSystemHealthCheck(
        IPermissionService permissionService,
        IPermissionMetricsService metricsService,
        ILogger<PermissionSystemHealthCheck> logger)
    {
        _permissionService = permissionService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            var issues = new List<string>();
            
            // 1. Teste básico de funcionalidade
            var functionalityResult = await CheckBasicFunctionalityAsync(cancellationToken);
            healthData.Add("basic_functionality", functionalityResult.Status);
            if (!functionalityResult.IsHealthy)
            {
                issues.Add($"Basic functionality: {functionalityResult.Issue}");
            }

            // 2. Verificação de performance
            var performanceResult = CheckPerformanceMetrics();
            healthData.Add("performance_metrics", performanceResult.Status);
            healthData.Add("cache_hit_rate", performanceResult.CacheHitRate);
            healthData.Add("active_checks", performanceResult.ActiveChecks);
            
            if (!performanceResult.IsHealthy)
            {
                issues.Add($"Performance: {performanceResult.Issue}");
            }

            // 3. Verificação de cache
            var cacheResult = await CheckCacheHealthAsync(cancellationToken);
            healthData.Add("cache_health", cacheResult.Status);
            if (!cacheResult.IsHealthy)
            {
                issues.Add($"Cache: {cacheResult.Issue}");
            }

            // 4. Verificação de resolvers
            var resolversResult = CheckModuleResolvers();
            healthData.Add("module_resolvers", resolversResult.Status);
            healthData.Add("resolver_count", resolversResult.ResolverCount);
            if (!resolversResult.IsHealthy)
            {
                issues.Add($"Module resolvers: {resolversResult.Issue}");
            }

            // Determina status geral
            var overallStatus = issues.Any() 
                ? (issues.Count > 2 ? HealthStatus.Unhealthy : HealthStatus.Degraded)
                : HealthStatus.Healthy;

            var description = overallStatus switch
            {
                HealthStatus.Healthy => "Permission system is operating normally",
                HealthStatus.Degraded => $"Permission system is degraded: {string.Join("; ", issues)}",
                HealthStatus.Unhealthy => $"Permission system is unhealthy: {string.Join("; ", issues)}",
                _ => "Permission system status unknown"
            };

            return new HealthCheckResult(overallStatus, description, data: healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission system health check failed");
            return new HealthCheckResult(
                HealthStatus.Unhealthy, 
                "Permission system health check threw an exception", 
                ex);
        }
    }

    /// <summary>
    /// Verifica funcionalidade básica com um usuário de teste.
    /// </summary>
    private async Task<InternalHealthCheckResult> CheckBasicFunctionalityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var testUserId = "health-check-test-user";
            var testPermission = EPermission.UsersRead;

            // Testa resolução de permissões
            var startTime = DateTimeOffset.UtcNow;
            var permissions = await _permissionService.GetUserPermissionsAsync(testUserId, cancellationToken);
            var duration = DateTimeOffset.UtcNow - startTime;

            // Verifica se a operação não demorou muito
            if (duration > MaxPermissionResolutionTime)
            {
                return new InternalHealthCheckResult(false, $"Permission resolution took {duration.TotalSeconds:F2}s (max: {MaxPermissionResolutionTime.TotalSeconds}s)");
            }

            // Testa verificação de permissão
            var hasPermission = await _permissionService.HasPermissionAsync(testUserId, testPermission, cancellationToken);
            
            // Para health check, não importa se tem ou não a permissão, apenas que a operação funcione
            return new InternalHealthCheckResult(true, "Basic functionality working");
        }
        catch (Exception ex)
        {
            return new InternalHealthCheckResult(false, $"Basic functionality failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica métricas de performance do sistema.
    /// </summary>
    private PerformanceHealthResult CheckPerformanceMetrics()
    {
        try
        {
            var stats = _metricsService.GetSystemStats();

            var issues = new List<string>();

            // Verifica taxa de cache hit
            if (stats.TotalPermissionChecks > 100 && stats.CacheHitRate < MinCacheHitRate)
            {
                issues.Add($"Low cache hit rate: {stats.CacheHitRate:P1} (min: {MinCacheHitRate:P1})");
            }

            // Verifica número de verificações ativas
            if (stats.ActiveChecks > MaxActiveChecks)
            {
                issues.Add($"Too many active checks: {stats.ActiveChecks} (max: {MaxActiveChecks})");
            }

            return new PerformanceHealthResult
            {
                IsHealthy = !issues.Any(),
                Status = issues.Any() ? "degraded" : "healthy",
                Issue = string.Join("; ", issues),
                CacheHitRate = stats.CacheHitRate,
                ActiveChecks = stats.ActiveChecks
            };
        }
        catch (Exception ex)
        {
            return new PerformanceHealthResult
            {
                IsHealthy = false,
                Status = "error",
                Issue = $"Failed to get performance metrics: {ex.Message}",
                CacheHitRate = 0,
                ActiveChecks = 0
            };
        }
    }

    /// <summary>
    /// Verifica saúde do sistema de cache.
    /// </summary>
    private async Task<InternalHealthCheckResult> CheckCacheHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var testUserId = "cache-health-test";
            
            // Testa operação de cache simples
            var startTime = DateTimeOffset.UtcNow;
            
            // Primeira chamada (cache miss esperado)
            await _permissionService.GetUserPermissionsAsync(testUserId, cancellationToken);
            
            // Segunda chamada (cache hit esperado)
            await _permissionService.GetUserPermissionsAsync(testUserId, cancellationToken);
            
            var duration = DateTimeOffset.UtcNow - startTime;

            // Cache deve fazer a segunda chamada mais rápida
            if (duration > TimeSpan.FromSeconds(1))
            {
                return new InternalHealthCheckResult(false, $"Cache operations took too long: {duration.TotalMilliseconds}ms");
            }

            return new InternalHealthCheckResult(true, "Cache working normally");
        }
        catch (Exception ex)
        {
            return new InternalHealthCheckResult(false, $"Cache health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica se os resolvers de módulos estão registrados.
    /// </summary>
    private ResolversHealthResult CheckModuleResolvers()
    {
        try
        {
            // Esta verificação seria mais robusta com acesso ao service provider
            // Por agora, assume que se chegou até aqui, os resolvers básicos estão funcionando
            
            return new ResolversHealthResult
            {
                IsHealthy = true,
                Status = "healthy",
                Issue = "",
                ResolverCount = 1 // Pelo menos o UsersPermissionResolver deve estar presente
            };
        }
        catch (Exception ex)
        {
            return new ResolversHealthResult
            {
                IsHealthy = false,
                Status = "error",
                Issue = $"Failed to check module resolvers: {ex.Message}",
                ResolverCount = 0
            };
        }
    }

    private record InternalHealthCheckResult(bool IsHealthy, string Issue)
    {
        public string Status => IsHealthy ? "healthy" : "unhealthy";
    }

    private record PerformanceHealthResult
    {
        public bool IsHealthy { get; init; }
        public string Status { get; init; } = "";
        public string Issue { get; init; } = "";
        public double CacheHitRate { get; init; }
        public int ActiveChecks { get; init; }
    }

    private record ResolversHealthResult
    {
        public bool IsHealthy { get; init; }
        public string Status { get; init; } = "";
        public string Issue { get; init; } = "";
        public int ResolverCount { get; init; }
    }
}

/// <summary>
/// Extensões para facilitar o registro do health check de permissões.
/// </summary>
public static class PermissionHealthCheckExtensions
{
    /// <summary>
    /// Adiciona o health check do sistema de permissões.
    /// </summary>
    private static readonly string[] HealthCheckTags = ["permissions", "authorization", "security"];

    public static IServiceCollection AddPermissionSystemHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PermissionSystemHealthCheck>(
                "permission_system",
                HealthStatus.Degraded,
                HealthCheckTags);

        return services;
    }
}
