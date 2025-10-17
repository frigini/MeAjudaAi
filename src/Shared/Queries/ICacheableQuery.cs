namespace MeAjudaAi.Shared.Queries;

/// <summary>
/// Interface para queries que podem ser cacheadas.
/// Implementar esta interface permite que a query seja automaticamente cacheada pelo CachingBehavior.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Gera uma chave única para o cache baseada nos parâmetros da query.
    /// </summary>
    /// <returns>Chave do cache</returns>
    string GetCacheKey();

    /// <summary>
    /// Define o tempo de expiração do cache.
    /// </summary>
    /// <returns>Tempo de expiração</returns>
    TimeSpan GetCacheExpiration();

    /// <summary>
    /// Define tags para invalidação em grupo do cache.
    /// </summary>
    /// <returns>Tags do cache</returns>
    IReadOnlyCollection<string>? GetCacheTags() => null;
}