using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Common;
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
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

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

        _logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

        // Tenta buscar no cache primeiro
        var cachedResult = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}. Executing query.", cacheKey);

        // Executa a query
        var result = await next();

        // Armazena no cache se o resultado não for nulo
        if (result != null)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = cacheExpiration,
                LocalCacheExpiration = TimeSpan.FromMinutes(5) // Cache local por 5 minutos
            };

            await _cacheService.SetAsync(cacheKey, result, cacheExpiration, options, cacheTags, cancellationToken);
            
            _logger.LogDebug("Cached result for key: {CacheKey} with expiration: {Expiration}", 
                cacheKey, cacheExpiration);
        }

        return result;
    }
}