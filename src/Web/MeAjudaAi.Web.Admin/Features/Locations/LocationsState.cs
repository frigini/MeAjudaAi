using Fluxor;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Locations;

/// <summary>
/// Estado Fluxor para gerenciamento de cidades permitidas (geographic restrictions).
/// Mantém lista de cidades carregadas do backend via ILocationsApi.
/// </summary>
[FeatureState]
public sealed record LocationsState
{
    /// <summary>Lista de cidades permitidas carregadas do backend</summary>
    public IReadOnlyList<ModuleAllowedCityDto> AllowedCities { get; init; } = [];

    /// <summary>Indica se está carregando cidades do backend</summary>
    public bool IsLoading { get; init; }

    /// <summary>Mensagem de erro caso operação falhe</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Indica se há erro ativo</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
