using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Command para criar nova cidade permitida
/// </summary>
public sealed record CreateAllowedCityCommand(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    double? Latitude = null,
    double? Longitude = null,
    double ServiceRadiusKm = 0,
    bool IsActive = true) : Command<Result<Guid>>;
