using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para atualização parcial de cidade permitida.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record PatchAllowedCityCommand(
    Guid Id,
    double? ServiceRadiusKm,
    bool? IsActive) : ICommand<Result>
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}
