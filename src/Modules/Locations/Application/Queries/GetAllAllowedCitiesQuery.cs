using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter todas as cidades permitidas.
/// </summary>
public sealed record GetAllAllowedCitiesQuery : Query<IReadOnlyList<AllowedCityDto>>
{
    public bool OnlyActive { get; init; } = true;
}
