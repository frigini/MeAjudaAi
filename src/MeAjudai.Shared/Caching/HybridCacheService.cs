using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudai.Shared.Caching;

public class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HybridCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Try memory cache first
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
        {
            return memoryValue;
        }

        // Try distributed cache
        try
        {
            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (distributedValue != null)
            {
                var value = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);

                // Store in memory cache for faster access
                _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));

                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get value from distributed cache for key {Key}", key);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var exp = expiration ?? TimeSpan.FromHours(1);

        // Set in memory cache
        _memoryCache.Set(key, value, TimeSpan.FromMinutes(Math.Min(exp.TotalMinutes, 30)));

        // Set in distributed cache
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = exp
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set value in distributed cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove value from distributed cache for key {Key}", key);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Memory cache doesn't support pattern removal easily
        // For distributed cache, this would need Redis-specific implementation
        _logger.LogWarning("Pattern removal not implemented for hybrid cache");
        return Task.CompletedTask;
    }
}
