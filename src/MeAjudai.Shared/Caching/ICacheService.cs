using Microsoft.Extensions.Caching.Hybrid;

namespace MeAjudai.Shared.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken cancellationToken = default);
}