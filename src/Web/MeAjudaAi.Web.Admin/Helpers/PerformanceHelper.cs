using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Performance monitoring and optimization utilities
/// </summary>
public static class PerformanceHelper
{
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
    /// Memoization cache for expensive computed properties
    /// </summary>
    private static readonly Dictionary<string, (object Value, DateTime CachedAt)> MemoizationCache = new();
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Memoize a function result with cache key and optional expiration
    /// </summary>
    public static T Memoize<T>(string cacheKey, Func<T> factory, TimeSpan? cacheDuration = null) where T : notnull
    {
        var duration = cacheDuration ?? DefaultCacheDuration;

        if (MemoizationCache.TryGetValue(cacheKey, out var cached))
        {
            // Check if cache is still valid
            if (DateTime.UtcNow - cached.CachedAt < duration)
            {
                return (T)cached.Value;
            }

            // Remove expired cache
            MemoizationCache.Remove(cacheKey);
        }

        // Compute and cache
        var value = factory();
        MemoizationCache[cacheKey] = (value, DateTime.UtcNow);
        return value;
    }

    /// <summary>
    /// Clear memoization cache for specific key or all
    /// </summary>
    public static void ClearMemoizationCache(string? cacheKey = null)
    {
        if (cacheKey != null)
        {
            MemoizationCache.Remove(cacheKey);
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
    /// Throttle function execution to prevent excessive calls
    /// </summary>
    private static readonly Dictionary<string, DateTime> ThrottleTimestamps = new();

    public static bool ShouldThrottle(string key, TimeSpan minInterval)
    {
        if (ThrottleTimestamps.TryGetValue(key, out var lastExecution))
        {
            if (DateTime.UtcNow - lastExecution < minInterval)
            {
                return true; // Throttled
            }
        }

        ThrottleTimestamps[key] = DateTime.UtcNow;
        return false; // Not throttled
    }

    /// <summary>
    /// Get performance metrics summary
    /// </summary>
    public static string GetCacheStatistics()
    {
        var totalCached = MemoizationCache.Count;
        var expiredCount = MemoizationCache.Count(x => DateTime.UtcNow - x.Value.CachedAt > DefaultCacheDuration);
        
        return $"Memoization Cache: {totalCached} items ({expiredCount} expired)";
    }
}
