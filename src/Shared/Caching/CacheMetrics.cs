using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;

namespace MeAjudaAi.Shared.Caching;

/// <summary>
/// Interface para métricas específicas de operações de cache.
/// </summary>
public interface ICacheMetrics
{
    void RecordCacheHit(string key, string operation = "get");
    void RecordCacheMiss(string key, string operation = "get");
    void RecordOperationDuration(double durationSeconds, string operation, string result);
    void RecordOperation(string key, string operation, bool isHit, double durationSeconds);
}

/// <summary>
/// Implementação concreta das métricas de cache utilizando System.Diagnostics.Metrics.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CacheMetrics : ICacheMetrics
{
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Counter<long> _cacheOperations;
    private readonly Histogram<double> _cacheOperationDuration;

    private static string NormalizeCacheKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "empty";

        var colonIndex = key.AsSpan().IndexOf(':');
        if (colonIndex >= 0)
        {
            var type = key.AsSpan()[..colonIndex];
            if (type.Equals("user".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return "user:{id}";
            if (type.Equals("provider".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return "provider:{id}";
            if (type.Equals("permission".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return "permission:{id}";
            if (type.Equals("role".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return "role:{id}";
            return $"{type.ToString()}:{{id}}";
        }

        if (key.Length > 20)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            var hash = Convert.ToHexString(hashBytes)[..8];
            return $"hash:{hash}";
        }

        return key;
    }

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

    public void RecordCacheHit(string key, string operation = "get")
    {
        var normalizedKey = NormalizeCacheKey(key);
        _cacheHits.Add(1, new KeyValuePair<string, object?>("key", normalizedKey),
                           new KeyValuePair<string, object?>("operation", operation));
        _cacheOperations.Add(1, new KeyValuePair<string, object?>("result", "hit"),
                                new KeyValuePair<string, object?>("operation", operation));
    }

    public void RecordCacheMiss(string key, string operation = "get")
    {
        var normalizedKey = NormalizeCacheKey(key);
        _cacheMisses.Add(1, new KeyValuePair<string, object?>("key", normalizedKey),
                            new KeyValuePair<string, object?>("operation", operation));
        _cacheOperations.Add(1, new KeyValuePair<string, object?>("result", "miss"),
                                new KeyValuePair<string, object?>("operation", operation));
    }

    public void RecordOperationDuration(double durationSeconds, string operation, string result)
    {
        _cacheOperationDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", result));
    }

    public void RecordOperation(string key, string operation, bool isHit, double durationSeconds)
    {
        if (isHit)
            RecordCacheHit(key, operation);
        else
            RecordCacheMiss(key, operation);

        RecordOperationDuration(durationSeconds, operation, isHit ? "hit" : "miss");
    }
}
