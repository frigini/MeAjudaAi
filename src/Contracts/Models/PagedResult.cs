namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Representa um resultado paginado de uma consulta à API.
/// </summary>
/// <typeparam name="T">Tipo dos itens na página</typeparam>
/// <remarks>
/// Usado para retornar listas paginadas com metadados de navegação,
/// permitindo implementar paginação no frontend de forma eficiente.
/// </remarks>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Itens da página atual
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Número da página atual (1-based)
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Tamanho da página (quantidade de itens por página)
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    public required int TotalItems { get; init; }

    /// <summary>
    /// Total de páginas disponíveis
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Indica se existe uma página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se existe uma próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
