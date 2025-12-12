using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Command para atualizar cidade permitida existente
/// </summary>
public sealed record UpdateAllowedCityCommand(
    Guid Id,
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive) : IRequest<Unit>;
