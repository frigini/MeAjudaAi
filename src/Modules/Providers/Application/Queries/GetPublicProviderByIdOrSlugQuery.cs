using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar dados públicos de um prestador por ID (GUID) ou slug.
/// Acessível sem autenticação.
/// </summary>
public sealed record GetPublicProviderByIdOrSlugQuery(string IdOrSlug, bool IsAuthenticated = false) : Query<Result<PublicProviderDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"provider:public:{IdOrSlug}:{(IsAuthenticated ? "auth" : "anon")}";

    // Cache de 10 minutos para dados públicos (bom para SEO e performance)
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(10);

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        if (Guid.TryParse(IdOrSlug, out var id))
            return ["providers", $"provider:{id}"];

        return ["providers", $"provider-slug:{IdOrSlug}"];
    }
}
