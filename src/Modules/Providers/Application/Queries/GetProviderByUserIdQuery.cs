using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestador de serviços por ID do usuário.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProviderByUserIdQuery(Guid UserId) : Query<Result<ProviderDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"provider:user:{UserId}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Providers, CacheTags.ProviderTag(UserId)];
}
