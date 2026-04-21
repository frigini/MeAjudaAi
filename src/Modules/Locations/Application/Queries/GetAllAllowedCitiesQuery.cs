using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter todas as cidades permitidas.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetAllAllowedCitiesQuery : Query<IReadOnlyList<AllowedCityDto>>
{
    public bool OnlyActive { get; init; } = true;
}
