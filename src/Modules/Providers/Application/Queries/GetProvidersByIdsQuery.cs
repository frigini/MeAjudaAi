using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar múltiplos prestadores de serviços por IDs.
/// </summary>
public sealed record GetProvidersByIdsQuery(IReadOnlyList<Guid> ProviderIds) : Query<Result<IReadOnlyList<ProviderDto>>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        // Ordena IDs para garantir cache hit independente da ordem
        var sortedIds = string.Join("-", ProviderIds.OrderBy(x => x));
        return $"providers:batch:{sortedIds}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 15 minutos para batch queries
        return TimeSpan.FromMinutes(15);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["providers", "providers-batch"];
    }
}
