using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;

/// <summary>
/// Query para obter todos os serviços, com a opção de filtrar apenas os ativos e/ou por nome.
/// </summary>
/// <param name="ActiveOnly"></param>
/// <param name="Name"></param>
[ExcludeFromCodeCoverage]
public sealed record GetAllServicesQuery(bool ActiveOnly = false, string? Name = null)
    : Query<Result<IReadOnlyList<ServiceListDto>>>, ICacheableQuery
{
    public string GetCacheKey() => $"services:all:active:{ActiveOnly}:name:{Name}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(2);
    public IReadOnlyCollection<string>? GetCacheTags() => [CacheTags.ServiceCatalogs];
}
