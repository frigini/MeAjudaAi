using Fluxor;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

namespace MeAjudaAi.Web.Admin.Features.ServiceCatalogs;

/// <summary>
/// Estado global para gestão de catálogo de serviços (categorias e serviços).
/// </summary>
[FeatureState]
public sealed record ServiceCatalogsState
{
    /// <summary>
    /// Lista de categorias carregadas
    /// </summary>
    public IReadOnlyList<ModuleServiceCategoryDto> Categories { get; init; } = Array.Empty<ModuleServiceCategoryDto>();

    /// <summary>
    /// Lista de serviços carregados
    /// </summary>
    public IReadOnlyList<ModuleServiceListDto> Services { get; init; } = Array.Empty<ModuleServiceListDto>();

    /// <summary>
    /// Indica se está carregando categorias
    /// </summary>
    public bool IsLoadingCategories { get; init; }

    /// <summary>
    /// Indica se está carregando serviços
    /// </summary>
    public bool IsLoadingServices { get; init; }

    /// <summary>
    /// Mensagem de erro caso ocorra falha
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Indica se houve erro
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
