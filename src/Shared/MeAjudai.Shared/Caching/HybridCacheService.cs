using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeAjudaAi.Shared.Caching;

public class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly CacheMetrics _metrics;

    public HybridCacheService(
        HybridCache hybridCache,
        ILogger<HybridCacheService> logger,
        CacheMetrics metrics)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var isHit = false;
        
        try
        {
            var result = await _hybridCache.GetOrCreateAsync<T>(
                key,
                factory: _ => 
                {
                    isHit = false; // Factory called = cache miss
                    return new ValueTask<T>(default(T)!);
                },
                cancellationToken: cancellationToken);
                
            // Se o factory não foi chamado, foi um hit
            if (!isHit && result != null && !result.Equals(default(T)))
            {
                isHit = true;
            }
            
            stopwatch.Stop();
            _metrics.RecordOperation(key, "get", isHit, stopwatch.Elapsed.TotalSeconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get", "error");
            _logger.LogWarning(ex, "Failed to get value from cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        HybridCacheEntryOptions? options = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            options ??= GetDefaultOptions(expiration);

            await _hybridCache.SetAsync(key, value, options, tags, cancellationToken);
            
            stopwatch.Stop();
            _metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "success");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "error");
            _logger.LogWarning(ex, "Failed to set value in cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hybridCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove value from cache for key {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hybridCache.RemoveByTagAsync(pattern, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove values by pattern {Pattern}", pattern);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration = null,
        HybridCacheEntryOptions? options = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var factoryCalled = false;
        
        try
        {
            options ??= GetDefaultOptions(expiration);

            var result = await _hybridCache.GetOrCreateAsync(
                key,
                async (ct) => 
                {
                    factoryCalled = true; // Factory chamado = cache miss
                    return await factory(ct);
                },
                options,
                tags,
                cancellationToken);
                
            stopwatch.Stop();
            _metrics.RecordOperation(key, "get-or-create", !factoryCalled, stopwatch.Elapsed.TotalSeconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get-or-create", "error");
            _logger.LogError(ex, "Failed to get or create cache value for key {Key}", key);
            return await factory(cancellationToken);
        }
    }

    private static HybridCacheEntryOptions GetDefaultOptions(TimeSpan? expiration = null)
    {
        return new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        };
    }
}