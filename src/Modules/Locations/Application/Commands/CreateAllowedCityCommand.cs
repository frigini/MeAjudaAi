using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Command para criar nova cidade permitida
/// </summary>
public sealed record CreateAllowedCityCommand(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive = true) : IRequest<Guid>;
