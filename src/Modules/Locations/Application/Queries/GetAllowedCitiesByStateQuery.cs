using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter cidades permitidas por estado.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetAllowedCitiesByStateQuery : Query<IReadOnlyList<AllowedCityDto>>, ICacheableQuery
{
    public string State { get; init; } = string.Empty;

    public string GetCacheKey() => $"locations:state:{State.Trim().ToUpperInvariant()}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? GetCacheTags() =>
        [CacheTags.Locations];
}
