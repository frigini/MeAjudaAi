using Fluxor;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;

namespace MeAjudaAi.Web.Admin.Features.Providers;

/// <summary>
/// Estado Fluxor para a feature de Providers.
/// Armazena a lista de providers, estado de loading e erros.
/// </summary>
[FeatureState]
public record ProvidersState
{
    /// <summary>
    /// Lista de providers carregados
    /// </summary>
    public IReadOnlyList<ModuleProviderDto> Providers { get; init; } = [];

    /// <summary>
    /// Indica se está carregando dados
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Número da página atual (paginação)
    /// </summary>
    public int CurrentPage { get; init; } = 1;

    /// <summary>
    /// Tamanho da página (paginação)
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Total de registros disponíveis
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages => TotalCount > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Indica se tem página anterior
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Indica se tem próxima página
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}
