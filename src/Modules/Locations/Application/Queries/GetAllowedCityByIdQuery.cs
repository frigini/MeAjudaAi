using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter uma cidade permitida por ID.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetAllowedCityByIdQuery(Guid Id) : Query<AllowedCityDto?>, ICacheableQuery
{
    public string GetCacheKey() => $"location:{Id}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(30);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Locations, CacheTags.MunicipioTag(Id.ToString())];
}
