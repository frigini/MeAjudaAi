using MeAjudaAi.Shared.Caching.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeAjudaAi.Shared.Caching;

public class HybridCacheService(HybridCache hybridCache,
    ILogger<HybridCacheService> logger,
    ICacheMetrics? metrics,
    IConfiguration configuration) : ICacheService
{
    private readonly bool _isCacheEnabled = configuration.GetValue<bool>("Cache:Enabled", true);
    private readonly ICacheMetrics? _metrics = metrics;
    
    public async Task<(T? value, bool isCached)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Se o cache estiver desabilitado, ignora e retorna cache miss
        if (!_isCacheEnabled)
        {
            logger.LogDebug("Cache desabilitado - ignorando cache para chave {Key}", key);
            return (default, false);
        }

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
            _metrics?.RecordOperation(key, "get", isCached, stopwatch.Elapsed.TotalSeconds);

            // Retornar tupla: (valor, estava_em_cache)
            return isCached ? (result, true) : (default, false);
        }
        catch (InvalidOperationException)
        {
            stopwatch.Stop();
            logger.LogDebug("Item não encontrado no cache para chave {Key} e valueFactory retornou null", key);
            return (default, false);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics?.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get", "error");
            logger.LogWarning(ex, "Falha ao obter valor do cache para chave {Key}", key);
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
        // Se o cache estiver desabilitado, ignora e não salva
        if (!_isCacheEnabled)
        {
            logger.LogDebug("Cache desabilitado - ignorando cache para chave {Key}", key);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            options ??= GetDefaultOptions(expiration);

            await hybridCache.SetAsync(key, value, options, tags, cancellationToken);

            stopwatch.Stop();
            _metrics?.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "success");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics?.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "set", "error");
            logger.LogWarning(ex, "Falha ao salvar valor no cache para chave {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Se o cache estiver desabilitado, ignora e não remove
        if (!_isCacheEnabled)
        {
            logger.LogDebug("Cache desabilitado - ignorando remoção para chave {Key}", key);
            return;
        }

        try
        {
            await hybridCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover valor do cache para chave {Key}", key);
        }
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Se o cache estiver desabilitado, ignora e não remove
        if (!_isCacheEnabled)
        {
            logger.LogDebug("Cache desabilitado - ignorando remoção para tag {Tag}", tag);
            return;
        }

        try
        {
            await hybridCache.RemoveByTagAsync(tag, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover valores pela tag {Tag}", tag);
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
        // Se o cache estiver desativado, ignore e chame a factory diretamente
        if (!_isCacheEnabled)
        {
            logger.LogDebug("Cache disabled - bypassing cache for key {Key}", key);
            return await factory(cancellationToken);
        }

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
            _metrics?.RecordOperation(key, "get-or-create", !factoryCalled, stopwatch.Elapsed.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics?.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "get-or-create", "error");
            logger.LogError(ex, "Failed to get or create cache value for key {Key}", key);
            return default!;
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
