using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Command para criar nova cidade permitida
/// </summary>
public sealed record CreateAllowedCityCommand(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    double Latitude = 0,
    double Longitude = 0,
    double ServiceRadiusKm = 0,
    bool IsActive = true) : Command<Result<Guid>>;
