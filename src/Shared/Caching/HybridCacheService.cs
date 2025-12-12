using System.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Caching;

public class HybridCacheService(
    HybridCache hybridCache,
    ILogger<HybridCacheService> logger,
    CacheMetrics metrics) : ICacheService
{
    public async Task<(T? value, bool isCached)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var factoryCalled = false;

        try
        {
            var result = await hybridCache.GetOrCreateAsync<T>(
                key,
                factory: _ =>
                {
                    factoryCalled = true; // Factory chamado = cache miss
                    return new ValueTask<T>(default(T)!);
                },
                cancellationToken: cancellationToken);

            // Se o factory foi chamado, foi um miss; caso contrário, hit
            var isCached = !factoryCalled;

            stopwatch.Stop();
            metrics.RecordOperation(key, "get", isCached, stopwatch.Elapsed.TotalSeconds);

            // Retornar tupla: (valor, estava_em_cache)
            return isCached ? (result, true) : (default, false);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get", "error");
            logger.LogWarning(ex, "Failed to get value from cache for key {Key}", key);
            return (default, false);
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

            await hybridCache.SetAsync(key, value, options, tags, cancellationToken);

            stopwatch.Stop();
            metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "success");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "error");
            logger.LogWarning(ex, "Failed to set value in cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await hybridCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove value from cache for key {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: HybridCache only supports tag-based removal, not pattern matching.
            // This currently delegates to RemoveByTagAsync, which may not provide
            // the expected wildcard pattern matching behavior defined in the interface.
            // Consider implementing proper pattern matching or using a different caching provider.
            await hybridCache.RemoveByTagAsync(pattern, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove values by pattern {Pattern}", pattern);
        }
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await hybridCache.RemoveByTagAsync(tag, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove values by tag {Tag}", tag);
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

            var result = await hybridCache.GetOrCreateAsync(
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
            metrics.RecordOperation(key, "get-or-create", !factoryCalled, stopwatch.Elapsed.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get-or-create", "error");
            logger.LogError(ex, "Failed to get or create cache value for key {Key}", key);
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
