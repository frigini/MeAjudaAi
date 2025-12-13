using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Mediator;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Behaviors;

/// <summary>
/// Behavior para caching automático de queries usando HybridCache.
/// Aplica cache apenas em queries que implementam ICacheableQuery.
/// </summary>
/// <typeparam name="TRequest">Tipo da query</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Só aplica cache se a query implementar ICacheableQuery
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = cacheableQuery.GetCacheKey();
        var cacheExpiration = cacheableQuery.GetCacheExpiration();
        var cacheTags = cacheableQuery.GetCacheTags();

        logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

        // Tenta buscar no cache primeiro
        var (cachedResult, isCached) = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (isCached)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            // Return cached value as-is (null is valid for reference types and Nullable<T>)
            return cachedResult;
        }

        logger.LogDebug("Cache miss for key: {CacheKey}. Executing query.", cacheKey);

        // Executa a query
        var result = await next();

        // Only cache non-null results to avoid caching failures/misses
        if (result is not null)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = cacheExpiration,
                LocalCacheExpiration = TimeSpan.FromMinutes(5) // Cache local por 5 minutos
            };

            await cacheService.SetAsync(cacheKey, result, cacheExpiration, options, cacheTags, cancellationToken);

            logger.LogDebug("Cached result for key: {CacheKey} with expiration: {Expiration}",
                cacheKey, cacheExpiration);
        }
        else
        {
            logger.LogDebug("Skipping cache for null result with key: {CacheKey}", cacheKey);
        }

        return result;
    }
}
