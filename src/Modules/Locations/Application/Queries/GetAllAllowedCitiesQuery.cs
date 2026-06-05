using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter todas as cidades permitidas.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetAllAllowedCitiesQuery : Query<IReadOnlyList<AllowedCityDto>>, ICacheableQuery
{
    public bool OnlyActive { get; init; } = true;
    public string GetCacheKey() => $"locations:all:active:{OnlyActive}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Locations];
}
