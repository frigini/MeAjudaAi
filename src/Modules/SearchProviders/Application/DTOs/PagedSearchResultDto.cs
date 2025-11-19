namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando resultados de busca paginados.
/// </summary>
/// <typeparam name="T">Tipo dos itens no conjunto de resultados</typeparam>
public sealed record PagedSearchResultDto<T>
{
    /// <summary>
    /// Lista de itens na página atual.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Número total de itens que correspondem aos critérios de busca.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Número da página atual (baseado em 1).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Número de itens por página.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Número total de páginas.
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica se há mais páginas disponíveis.
    /// </summary>
    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;

    /// <summary>
    /// Indica se há uma página anterior.
    /// </summary>
    public bool HasPreviousPage => TotalPages > 0 && PageNumber > 1;
}
