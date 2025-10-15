using MeAjudaAi.Shared.Caching;
using System.Collections.Concurrent;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

/// <summary>
/// Implementação simples de ICacheService para testes
/// Usa ConcurrentDictionary em memória para simular cache
/// </summary>
internal class TestCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return _cache.TryGetValue(key, out var value) && value is T typedValue
             ? Task.FromResult<T?>(typedValue)
             : Task.FromResult<T?>(default);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration = null,
        Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var existingValue) && existingValue is T typedValue)
        {
            return typedValue;
        }

        var value = await factory(cancellationToken);
        _cache[key] = value!;
        return value;
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        _cache[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Para teste, remove todas as chaves que contenham a tag
        var keysToRemove = _cache.Keys.Where(k => k.Contains(tag, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _cache.Keys.Where(k => IsMatch(k, pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.ContainsKey(key));
    }

    private static bool IsMatch(string key, string pattern)
    {
        // Implementação simples de pattern matching
        if (pattern.Contains('*', StringComparison.Ordinal))
        {
            var parts = pattern.Split('*', StringSplitOptions.RemoveEmptyEntries);
            return parts.All(part => key.Contains(part, StringComparison.OrdinalIgnoreCase));
        }
        return key.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}