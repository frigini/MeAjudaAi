using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Commands;

/// <summary>
/// Comando para atualizar uma cidade permitida existente.
/// </summary>
public sealed record UpdateAllowedCityCommand : Command
{
    public Guid Id { get; init; }
    public string CityName { get; init; } = string.Empty;
    public string StateSigla { get; init; } = string.Empty;
    public int? IbgeCode { get; init; }
    public bool IsActive { get; init; }
}
