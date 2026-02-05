namespace MeAjudaAi.Contracts.Models;

/// <summary>
/// Envelope padrão para respostas paginadas da API
/// </summary>
public sealed record PagedResponse<T>
{
    /// <summary>
    /// Dados da resposta
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Total de itens disponíveis
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Página atual (1-based)
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PagedResponse(T? data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}
