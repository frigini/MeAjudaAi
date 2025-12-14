using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter uma cidade permitida por ID.
/// </summary>
public sealed record GetAllowedCityByIdQuery : Query<AllowedCityDto?>
{
    public Guid Id { get; init; }
}
