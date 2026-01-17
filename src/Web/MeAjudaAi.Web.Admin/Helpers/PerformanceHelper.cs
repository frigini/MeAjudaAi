using System.Collections.Concurrent;
using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Utilitários de monitoramento e otimização de performance com caches limitados para prevenir vazamentos de memória.
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Número máximo de itens em cache antes da remoção LRU entrar em ação.
    /// </summary>
    private const int MaxCacheSize = 500;

    /// <summary>
    /// Número máximo de timestamps de throttle antes da limpeza.
    /// </summary>
    private const int MaxThrottleSize = 100;
    /// <summary>
    /// Mede o tempo de execução de uma ação.
    /// </summary>
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Mede o tempo de execução de uma ação.
    /// </summary>
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Cache de memoização para propriedades computadas com custo alto.
    /// Thread-safe com remoção LRU quando tamanho máximo (500) é atingido.
    /// </summary>
    private static readonly ConcurrentDictionary<string, CacheEntry> MemoizationCache = new();
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    private record CacheEntry(object Value, DateTime CachedAt, DateTime LastAccessedAt);

    /// <summary>
    /// Memoriza resultado de função com chave de cache e expiração opcional.
    /// Thread-safe com remoção automática quando cache excede 500 entradas.
    /// </summary>
    public static T Memoize<T>(string cacheKey, Func<T> factory, TimeSpan? cacheDuration = null) where T : notnull
    {
        var duration = cacheDuration ?? DefaultCacheDuration;

        if (MemoizationCache.TryGetValue(cacheKey, out var cached))
        {
            // Check if cache is still valid
            if (DateTime.UtcNow - cached.CachedAt < duration)
            {
                // Update last accessed timestamp for LRU
                var updated = cached with { LastAccessedAt = DateTime.UtcNow };
                MemoizationCache.TryUpdate(cacheKey, updated, cached);
                return (T)cached.Value;
            }

            // Remove expired cache
            MemoizationCache.TryRemove(cacheKey, out _);
        }

        // Compute and cache
        var value = factory();
        var entry = new CacheEntry(value, DateTime.UtcNow, DateTime.UtcNow);
        MemoizationCache[cacheKey] = entry;

        // Evict LRU entries if cache is too large
        EvictLRUIfNeeded(MemoizationCache, MaxCacheSize);

        return value;
    }

    /// <summary>
    /// Remove entradas menos recentemente usadas quando cache excede tamanho máximo.
    /// </summary>
    private static void EvictLRUIfNeeded(ConcurrentDictionary<string, CacheEntry> cache, int maxSize)
    {
        if (cache.Count <= maxSize) return;

        // Find and remove oldest entries (20% of max size)
        var entriesToRemove = cache.Count - maxSize + (int)(maxSize * 0.2);
        var oldestKeys = cache
            .OrderBy(x => x.Value.LastAccessedAt)
            .Take(entriesToRemove)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in oldestKeys)
        {
            cache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Limpa cache de memoização para chave específica ou todas.
    /// Operação thread-safe.
    /// </summary>
    public static void ClearMemoizationCache(string? cacheKey = null)
    {
        if (cacheKey != null)
        {
            MemoizationCache.TryRemove(cacheKey, out _);
        }
        else
        {
            MemoizationCache.Clear();
        }
    }

    /// <summary>
    /// Processa itens em lotes para prevenir bloqueio da UI.
    /// </summary>
    public static async Task ProcessInBatchesAsync<T>(
        IEnumerable<T> items, 
        Func<T, Task> processor, 
        int batchSize = 50,
        int delayBetweenBatches = 10)
    {
        var itemList = items.ToList();
        var totalBatches = (int)Math.Ceiling(itemList.Count / (double)batchSize);

        for (var i = 0; i < totalBatches; i++)
        {
            var batch = itemList.Skip(i * batchSize).Take(batchSize);
            await Task.WhenAll(batch.Select(processor));

            // Small delay to prevent UI blocking
            if (i < totalBatches - 1)
            {
                await Task.Delay(delayBetweenBatches);
            }
        }
    }

    /// <summary>
    /// Limita execução de função para prevenir chamadas excessivas.
    /// Thread-safe com limpeza automática ao exceder 100 entradas.
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> ThrottleTimestamps = new();

    public static bool ShouldThrottle(string key, TimeSpan minInterval)
    {
        var now = DateTime.UtcNow;

        if (ThrottleTimestamps.TryGetValue(key, out var lastExecution))
        {
            if (now - lastExecution < minInterval)
            {
                return true; // Throttled
            }
        }

        ThrottleTimestamps[key] = now;

        // Cleanup old entries if too many
        if (ThrottleTimestamps.Count > MaxThrottleSize)
        {
            var oldKeys = ThrottleTimestamps
                .Where(x => now - x.Value > TimeSpan.FromMinutes(10))
                .Select(x => x.Key)
                .ToList();

            foreach (var oldKey in oldKeys)
            {
                ThrottleTimestamps.TryRemove(oldKey, out _);
            }
        }

        return false; // Not throttled
    }

    /// <summary>
    /// Get performance metrics summary.
    /// Thread-safe snapshot of current cache state.
    /// </summary>
    public static string GetCacheStatistics()
    {
        var totalCached = MemoizationCache.Count;
        var expiredCount = MemoizationCache.Count(x => DateTime.UtcNow - x.Value.CachedAt > DefaultCacheDuration);
        
        return $"Memoization Cache: {totalCached}/{MaxCacheSize} items ({expiredCount} expired), Throttle: {ThrottleTimestamps.Count}/{MaxThrottleSize} entries";
    }
}
