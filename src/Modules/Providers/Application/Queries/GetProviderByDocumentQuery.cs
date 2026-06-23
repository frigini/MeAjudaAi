using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar um prestador de serviços por documento.
/// </summary>
/// <param name="Document">Número do documento do prestador</param>
[ExcludeFromCodeCoverage]
public sealed record GetProviderByDocumentQuery(string Document) : Query<Result<ProviderDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"provider:doc:{Document}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Providers];
}