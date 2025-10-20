using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Caching;

/// <summary>
/// Métricas específicas para operações de cache.
/// Fornece instrumentação para monitoramento de performance de cache.
/// </summary>
public sealed class CacheMetrics
{
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Counter<long> _cacheOperations;
    private readonly Histogram<double> _cacheOperationDuration;

    public CacheMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MeAjudaAi.Cache");

        _cacheHits = meter.CreateCounter<long>(
            "cache_hits_total",
            description: "Total number of cache hits");

        _cacheMisses = meter.CreateCounter<long>(
            "cache_misses_total",
            description: "Total number of cache misses");

        _cacheOperations = meter.CreateCounter<long>(
            "cache_operations_total",
            description: "Total number of cache operations");

        _cacheOperationDuration = meter.CreateHistogram<double>(
            "cache_operation_duration_seconds",
            unit: "s",
            description: "Duration of cache operations in seconds");
    }

    /// <summary>
    /// Registra um cache hit
    /// </summary>
    public void RecordCacheHit(string key, string operation = "get")
    {
        _cacheHits.Add(1, new KeyValuePair<string, object?>("key", key),
                           new KeyValuePair<string, object?>("operation", operation));
        _cacheOperations.Add(1, new KeyValuePair<string, object?>("result", "hit"),
                                new KeyValuePair<string, object?>("operation", operation));
    }

    /// <summary>
    /// Registra um cache miss
    /// </summary>
    public void RecordCacheMiss(string key, string operation = "get")
    {
        _cacheMisses.Add(1, new KeyValuePair<string, object?>("key", key),
                            new KeyValuePair<string, object?>("operation", operation));
        _cacheOperations.Add(1, new KeyValuePair<string, object?>("result", "miss"),
                                new KeyValuePair<string, object?>("operation", operation));
    }

    /// <summary>
    /// Registra a duração de uma operação de cache
    /// </summary>
    public void RecordOperationDuration(double durationSeconds, string operation, string result)
    {
        _cacheOperationDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", result));
    }

    /// <summary>
    /// Registra uma operação de cache com todas as métricas
    /// </summary>
    public void RecordOperation(string key, string operation, bool isHit, double durationSeconds)
    {
        if (isHit)
            RecordCacheHit(key, operation);
        else
            RecordCacheMiss(key, operation);

        RecordOperationDuration(durationSeconds, operation, isHit ? "hit" : "miss");
    }
}
