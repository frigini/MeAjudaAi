using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para deletar uma cidade permitida.
/// </summary>
public sealed record DeleteAllowedCityCommand : Command
{
    public Guid Id { get; init; }
}
