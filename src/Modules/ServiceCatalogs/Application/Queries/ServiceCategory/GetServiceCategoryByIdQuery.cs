using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

/// <summary>
/// Query to retrieve a service category by its identifier.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetServiceCategoryByIdQuery(Guid Id)
    : Query<Result<ServiceCategoryDto?>>, ICacheableQuery
{
    public string GetCacheKey() => $"category:{Id}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.ServiceCatalogs, CacheTags.CategoryTag(Id)];
}
