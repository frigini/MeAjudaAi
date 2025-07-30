using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Caching;

public class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(
        HybridCache hybridCache,
        ILogger<HybridCacheService> logger)
    {
        _hybridCache = hybridCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _hybridCache.GetOrCreateAsync<T>(
                key,
                factory: _ => new ValueTask<T>(default(T)!),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
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
        try
        {
            options ??= GetDefaultOptions(expiration);

            await _hybridCache.SetAsync(key, value, options, tags, cancellationToken);
        }
        catch (Exception ex)
        {
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
        try
        {
            options ??= GetDefaultOptions(expiration);

            return await _hybridCache.GetOrCreateAsync(
                key,
                factory,
                options,
                tags,
                cancellationToken);
        }
        catch (Exception ex)
        {
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