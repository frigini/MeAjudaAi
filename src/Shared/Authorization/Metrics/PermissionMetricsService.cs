using Microsoft.Extensions.DependencyInjection;
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
    
    // Counters
    private readonly Counter<long> _permissionResolutionCounter;
    private readonly Counter<long> _permissionCheckCounter;
    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;
    private readonly Counter<long> _authorizationFailureCounter;
    
    // Histograms
    private readonly Histogram<double> _permissionResolutionDuration;
    private readonly Histogram<double> _cacheOperationDuration;
    private readonly Histogram<double> _authorizationCheckDuration;
    
    // Gauges (via ObservableGauge)
    private readonly ObservableGauge<int> _activePermissionChecks;
    private readonly ObservableGauge<double> _cacheHitRate;
    
    // State tracking
    private long _totalPermissionChecks;
    private long _totalCacheHits;
    private int _currentActiveChecks;
    private readonly Lock _statsLock = new();

    public PermissionMetricsService(ILogger<PermissionMetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("MeAjudaAi.Authorization", "1.0.0");
        
        // Initialize counters
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
        
        // Initialize histograms
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
        
        // Initialize observable gauges
        _activePermissionChecks = _meter.CreateObservableGauge<int>(
            "meajudaai_active_permission_checks",
            () => _currentActiveChecks,
            description: "Number of currently active permission checks");
            
        _cacheHitRate = _meter.CreateObservableGauge<double>(
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
            { "user_id", userId },
            { "module", module ?? "unknown" }
        };

        _permissionResolutionCounter.Add(1, tags);
        
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
            { "user_id", userId },
            { "permission", permission.GetValue() },
            { "module", permission.GetModule() },
            { "granted", granted.ToString() }
        };

        _permissionCheckCounter.Add(1, tags);
        
        if (!granted)
        {
            _authorizationFailureCounter.Add(1, tags);
        }

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
            { "user_id", userId },
            { "module", moduleName }
        };

        _permissionResolutionCounter.Add(1, tags);
        
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
    public IDisposable MeasureCacheOperation(string operation, bool hit)
    {
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

        return new OperationTimer(
            () => { },
            duration => _cacheOperationDuration.Record(duration.TotalSeconds, tags));
    }

    /// <summary>
    /// Registra falhas de autorização com detalhes.
    /// </summary>
    public void RecordAuthorizationFailure(string userId, EPermission permission, string reason)
    {
        var tags = new TagList
        {
            { "user_id", userId },
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
            { "user_id", userId },
            { "reason", reason }
        };

        // Use counter for cache invalidations
        _meter.CreateCounter<long>("meajudaai_permission_cache_invalidations_total")
              .Add(1, tags);
              
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

        _meter.CreateHistogram<double>($"meajudaai_permission_{component}_performance")
              .Record(value, tags);
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

    /// <summary>
    /// Timer para medir duração de operações.
    /// </summary>
    private sealed class OperationTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<TimeSpan> _onComplete;
        private bool _disposed;

        public OperationTimer(Action onStart, Action<TimeSpan> onComplete)
        {
            _onComplete = onComplete;
            _stopwatch = Stopwatch.StartNew();
            onStart();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _stopwatch.Stop();
            _onComplete(_stopwatch.Elapsed);
        }
    }
}

/// <summary>
/// Estatísticas do sistema de permissões.
/// </summary>
public sealed class PermissionSystemStats
{
    public long TotalPermissionChecks { get; init; }
    public long TotalCacheHits { get; init; }
    public double CacheHitRate { get; init; }
    public int ActiveChecks { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

/// <summary>
/// Extensões para facilitar o uso de métricas de permissões.
/// </summary>
public static class PermissionMetricsExtensions
{
    /// <summary>
    /// Adiciona o serviço de métricas de permissões ao DI.
    /// </summary>
    public static IServiceCollection AddPermissionMetrics(this IServiceCollection services)
    {
        services.AddSingleton<PermissionMetricsService>();
        services.AddSingleton<IPermissionMetricsService>(provider => provider.GetRequiredService<PermissionMetricsService>());
        return services;
    }

    /// <summary>
    /// Wrapper para medir operações de permissão com using statement.
    /// </summary>
    public static async Task<T> MeasureAsync<T>(
        this IPermissionMetricsService metrics,
        Func<Task<T>> operation,
        string operationType,
        string userId)
    {
        using var timer = metrics.MeasurePermissionResolution(userId, operationType);
        return await operation();
    }
}