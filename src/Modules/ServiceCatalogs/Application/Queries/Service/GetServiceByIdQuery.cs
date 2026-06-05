using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;

[ExcludeFromCodeCoverage]

public sealed record GetServiceByIdQuery(Guid Id) : Query<Result<ServiceDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"service:{Id}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.ServiceCatalogs, CacheTags.ServiceTag(Id)];
}
