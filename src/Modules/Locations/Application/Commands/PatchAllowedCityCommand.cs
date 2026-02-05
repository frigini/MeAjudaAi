using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para atualização parcial de cidade permitida.
/// </summary>
public sealed record PatchAllowedCityCommand(
    Guid Id,
    double? ServiceRadiusKm,
    bool? IsActive) : ICommand<Result>
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}
