using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar dados públicos de um prestador pelo ID.
/// Acessível sem autenticação.
/// </summary>
public sealed record GetPublicProviderByIdQuery(Guid Id, bool IsAuthenticated = false) : Query<Result<PublicProviderDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"provider:public:{Id}:{(IsAuthenticated ? "auth" : "anon")}";
    
    // Cache de 10 minutos para dados públicos (bom para SEO e performance)
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(10);
    
    public IReadOnlyCollection<string>? GetCacheTags() => ["providers", $"provider:{Id}"];
}
