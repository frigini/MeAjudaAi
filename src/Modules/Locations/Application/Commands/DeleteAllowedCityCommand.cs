using MeAjudaAi.Shared.Commands;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para deletar uma cidade permitida.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeleteAllowedCityCommand : Command
{
    public Guid Id { get; init; }
}
