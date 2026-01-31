using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para atualizar uma cidade permitida existente.
/// </summary>
public sealed record UpdateAllowedCityCommand : Command<Result>
{
    public Guid Id { get; init; }
    public string CityName { get; init; } = string.Empty;
    public string StateSigla { get; init; } = string.Empty;
    public int? IbgeCode { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double ServiceRadiusKm { get; init; }
    public bool IsActive { get; init; }
}
