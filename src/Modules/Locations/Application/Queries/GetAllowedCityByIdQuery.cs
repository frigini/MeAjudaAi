using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Queries;

/// <summary>
/// Query para obter uma cidade permitida por ID.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetAllowedCityByIdQuery : Query<AllowedCityDto?>
{
    public Guid Id { get; init; }
}
