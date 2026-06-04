using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Authorization.Metrics; // Import central timer
using MeAjudaAi.Shared.Authorization.Metrics.Models; // Import models namespace
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Authorization.Metrics;

/// <summary>
/// Serviço para métricas e monitoramento do sistema de permissões.
/// Coleta dados de performance, uso e falhas para observabilidade.
/// </summary>
public sealed class PermissionMetricsService : IPermissionMetricsService
{
    private readonly ILogger<PermissionMetricsService> _logger;
    private readonly Meter _meter;

    // Contadores
    private readonly Counter<long> _permissionResolutionCounter;
    private readonly Counter<long> _permissionCheckCounter;
    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;
    private readonly Counter<long> _authorizationFailureCounter;
    private readonly Counter<long> _cacheInvalidationCounter;

    // Histogramas
    private readonly Histogram<double> _permissionResolutionDuration;
    private readonly Histogram<double> _cacheOperationDuration;
    private readonly Histogram<double> _authorizationCheckDuration;
    private readonly Histogram<double> _performanceHistogram;

    // Rastreamento de estado
    private long _totalPermissionChecks;
    private long _totalCacheHits;
    private int _currentActiveChecks;
    private readonly object _statsLock = new();

    public PermissionMetricsService(ILogger<PermissionMetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("MeAjudaAi.Authorization", "1.0.0");

        // Inicializa contadores
        _permissionResolutionCounter = _meter.CreateCounter<long>(
            "meajudaai_permission_resolutions_total",
            description: "Total number of permission resolutions performed");

        _permissionCheckCounter = _meter.CreateCounter<long>(
            "meajudaai_permission_checks_total",
            description: "Total number of permission checks performed");

        _cacheHitCounter = _meter.CreateCounter<long>(
            "meajudaai_permission_cache_hits_total",
            description: "Total number of permission cache hits");

        _cacheMissCounter = _meter.CreateCounter<long>(
            "meajudaai_permission_cache_misses_total",
            description: "Total number of permission cache misses");

        _authorizationFailureCounter = _meter.CreateCounter<long>(
            "meajudaai_authorization_failures_total",
            description: "Total number of authorization failures");

        _cacheInvalidationCounter = _meter.CreateCounter<long>(
            "meajudaai_permission_cache_invalidations_total",
            description: "Total number of permission cache invalidations");

        // Inicializa histogramas
        _permissionResolutionDuration = _meter.CreateHistogram<double>(
            "meajudaai_permission_resolution_duration_seconds",
            "seconds",
            "Duration of permission resolution operations");

        _cacheOperationDuration = _meter.CreateHistogram<double>(
            "meajudaai_permission_cache_operation_duration_seconds",
            "seconds",
            "Duration of permission cache operations");

        _authorizationCheckDuration = _meter.CreateHistogram<double>(
            "meajudaai_authorization_check_duration_seconds",
            "seconds",
            "Duration of authorization checks");

        _performanceHistogram = _meter.CreateHistogram<double>(
            "meajudaai_permission_performance",
            description: "Performance metrics for permission components");

        // Inicializa medidores observáveis diretamente (não é necessário armazenar referência)
        _meter.CreateObservableGauge<int>(
            "meajudaai_active_permission_checks",
            () => _currentActiveChecks,
            description: "Number of currently active permission checks");

        _meter.CreateObservableGauge<double>(
            "meajudaai_permission_cache_hit_rate",
            () => CalculateCacheHitRate(),
            description: "Permission cache hit rate (0-1)");
    }

    /// <summary>
    /// Registra uma operação de resolução de permissões.
    /// </summary>
    public IDisposable MeasurePermissionResolution(string userId, string? module = null)
    {
        var tags = new TagList
        {
            { "module", module ?? "unknown" }
        };

        _permissionResolutionCounter.Add(1, tags);

        _logger.LogDebug("Permission resolution started for user {UserId} in module {Module}",
            userId, module ?? "unknown");

        return new OperationTimer(
            () => Interlocked.Increment(ref _currentActiveChecks),
            duration =>
            {
                Interlocked.Decrement(ref _currentActiveChecks);
                _permissionResolutionDuration.Record(duration.TotalSeconds, tags);

                if (duration.TotalMilliseconds > 1000) // Log slow operations
                {
                    _logger.LogWarning("Slow permission resolution: {Duration}ms for user {UserId} in module {Module}",
                        duration.TotalMilliseconds, userId, module);
                }
            });
    }

    /// <summary>
    /// Registra uma verificação de permissão.
    /// </summary>
    public IDisposable MeasurePermissionCheck(string userId, EPermission permission, bool granted)
    {
        var tags = new TagList
        {
            { "permission", permission.GetValue() },
            { "module", permission.GetModule() },
            { "result", granted ? "granted" : "denied" }
        };

        _permissionCheckCounter.Add(1, tags);

        if (!granted)
        {
            _authorizationFailureCounter.Add(1, tags);
        }

        _logger.LogDebug("Permission check: User {UserId} {Result} for permission {Permission}",
            userId, granted ? "granted" : "denied", permission.GetValue());

        lock (_statsLock)
        {
            _totalPermissionChecks++;
        }

        return new OperationTimer(
            () => Interlocked.Increment(ref _currentActiveChecks),
            duration =>
            {
                Interlocked.Decrement(ref _currentActiveChecks);
                _authorizationCheckDuration.Record(duration.TotalSeconds, tags);
            });
    }

    /// <summary>
    /// Registra uma verificação de múltiplas permissões.
    /// </summary>
    public IDisposable MeasureMultiplePermissionCheck(string userId, IEnumerable<EPermission> permissions, bool requireAll)
    {
        var permissionList = permissions.ToList();
        var tags = new TagList
        {
            { "user_id", userId },
            { "permission_count", permissionList.Count.ToString() },
            { "require_all", requireAll.ToString() }
        };

        _permissionCheckCounter.Add(permissionList.Count, tags);

        lock (_statsLock)
        {
            _totalPermissionChecks += permissionList.Count;
        }

        return new OperationTimer(
            () => Interlocked.Increment(ref _currentActiveChecks),
            duration =>
            {
                Interlocked.Decrement(ref _currentActiveChecks);
                _authorizationCheckDuration.Record(duration.TotalSeconds, tags);
            });
    }

    /// <summary>
    /// Registra uma operação de resolução de permissões por módulo.
    /// </summary>
    public IDisposable MeasureModulePermissionResolution(string userId, string moduleName)
    {
        var tags = new TagList
        {
            { "module", moduleName }
        };

        _permissionResolutionCounter.Add(1, tags);

        _logger.LogDebug("Module permission resolution started for user {UserId} in module {ModuleName}",
            userId, moduleName);

        return new OperationTimer(
            () => Interlocked.Increment(ref _currentActiveChecks),
            duration =>
            {
                Interlocked.Decrement(ref _currentActiveChecks);
                _permissionResolutionDuration.Record(duration.TotalSeconds, tags);

                if (duration.TotalMilliseconds > 1000) // Log slow operations
                {
                    _logger.LogWarning("Slow module permission resolution: {Duration}ms for user {UserId} in module {Module}",
                        duration.TotalMilliseconds, userId, moduleName);
                }
            });
    }

    /// <summary>
    /// Registra uma operação de cache.
    /// </summary>
    public IDisposable MeasureCacheOperation(string operation, Func<bool> getCacheHit)
    {
        return new OperationTimer(
            () => { },
            duration =>
            {
                var hit = getCacheHit();
                var tags = new TagList
                {
                    { "operation", operation },
                    { "result", hit ? "hit" : "miss" }
                };

                if (hit)
                {
                    _cacheHitCounter.Add(1, tags);
                    lock (_statsLock)
                    {
                        _totalCacheHits++;
                    }
                }
                else
                {
                    _cacheMissCounter.Add(1, tags);
                }
                
                _cacheOperationDuration.Record(duration.TotalSeconds, tags);
            });
    }

    /// <summary>
    /// Registra falhas de autorização com detalhes.
    /// </summary>
    public void RecordAuthorizationFailure(string userId, EPermission permission, string reason)
    {
        var tags = new TagList
        {
            { "permission", permission.GetValue() },
            { "module", permission.GetModule() },
            { "reason", reason }
        };

        _authorizationFailureCounter.Add(1, tags);

        _logger.LogWarning("Authorization failure: User {UserId} denied {Permission} - {Reason}",
            userId, permission.GetValue(), reason);
    }

    /// <summary>
    /// Registra eventos de invalidação de cache.
    /// </summary>
    public void RecordCacheInvalidation(string userId, string reason)
    {
        var tags = new TagList
        {
            { "reason", reason }
        };

        // Use existing counter field instead of creating new one
        _cacheInvalidationCounter.Add(1, tags);

        _logger.LogDebug("Permission cache invalidated for user {UserId}: {Reason}", userId, reason);
    }

    /// <summary>
    /// Registra estatísticas de performance do sistema.
    /// </summary>
    public void RecordPerformanceStats(string component, double value, string unit = "count")
    {
        var tags = new TagList
        {
            { "component", component },
            { "unit", unit }
        };

        _performanceHistogram.Record(value, tags);
    }

    /// <summary>
    /// Calcula a taxa de acerto do cache.
    /// </summary>
    private double CalculateCacheHitRate()
    {
        lock (_statsLock)
        {
            if (_totalPermissionChecks == 0)
                return 0.0;

            return (double)_totalCacheHits / _totalPermissionChecks;
        }
    }

    /// <summary>
    /// Obtém estatísticas resumidas do sistema de permissões.
    /// </summary>
    public PermissionSystemStats GetSystemStats()
    {
        lock (_statsLock)
        {
            return new PermissionSystemStats
            {
                TotalPermissionChecks = _totalPermissionChecks,
                TotalCacheHits = _totalCacheHits,
                CacheHitRate = CalculateCacheHitRate(),
                ActiveChecks = _currentActiveChecks,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }

    public void Dispose()
    {
        _meter.Dispose();
    }
}
