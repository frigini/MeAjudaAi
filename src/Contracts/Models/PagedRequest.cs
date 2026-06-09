using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Representa parâmetros de paginação para requisições paginadas.
/// </summary>
public abstract record PagedRequest
{
    /// <summary>
    /// Define o número de itens por página. O valor padrão é <see cref="Pagination.DefaultPageSize"/>.
    /// </summary>
    public int PageSize { get; init; } = Pagination.DefaultPageSize;

    /// <summary>
    /// Indica a página atual (base 1). O valor padrão é <see cref="Pagination.DefaultPageNumber"/>.
    /// </summary>
    public int PageNumber { get; init; } = Pagination.DefaultPageNumber;
}