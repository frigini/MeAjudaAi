using Fluxor;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Modules.Locations;

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

    /// <summary>Indica se está excluindo uma cidade</summary>
    public bool IsDeletingCity { get; init; }

    /// <summary>ID da cidade sendo excluída</summary>
    public Guid? DeletingCityId { get; init; }

    /// <summary>Indica se está alternando status de cidade</summary>
    public bool IsTogglingCity { get; init; }

    /// <summary>ID da cidade tendo status alternado</summary>
    public Guid? TogglingCityId { get; init; }
}
