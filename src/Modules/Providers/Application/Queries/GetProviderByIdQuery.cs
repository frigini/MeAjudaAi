using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestador de serviços por ID.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProviderByIdQuery(Guid ProviderId) : Query<Result<ProviderDto?>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return $"provider:id:{ProviderId}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache por 15 minutos para dados de prestador individual
        return TimeSpan.FromMinutes(15);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["providers", $"provider:{ProviderId}"];
    }
}
