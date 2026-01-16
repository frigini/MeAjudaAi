using System.Collections.Concurrent;
using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Performance monitoring and optimization utilities with bounded caches to prevent memory leaks.
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Maximum number of cached items before LRU eviction kicks in.
    /// </summary>
    private const int MaxCacheSize = 500;

    /// <summary>
    /// Maximum number of throttle timestamps before cleanup.
    /// </summary>
    private const int MaxThrottleSize = 100;
    /// <summary>
    /// Measure execution time of an action
    /// </summary>
    public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Measure execution time of an action
    /// </summary>
    public static (T Result, TimeSpan Duration) Measure<T>(Func<T> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Memoization cache for expensive computed properties.
    /// Thread-safe with LRU eviction when max size (500) is reached.
    /// </summary>
    private static readonly ConcurrentDictionary<string, CacheEntry> MemoizationCache = new();
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    private record CacheEntry(object Value, DateTime CachedAt, DateTime LastAccessedAt);

    /// <summary>
    /// Memoize a function result with cache key and optional expiration.
    /// Thread-safe with automatic eviction when cache exceeds 500 entries.
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
    /// Evict least recently used entries when cache exceeds max size.
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
    /// Clear memoization cache for specific key or all.
    /// Thread-safe operation.
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
    /// Batch process items to prevent UI blocking
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
    /// Throttle function execution to prevent excessive calls.
    /// Thread-safe with automatic cleanup when exceeding 100 entries.
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
