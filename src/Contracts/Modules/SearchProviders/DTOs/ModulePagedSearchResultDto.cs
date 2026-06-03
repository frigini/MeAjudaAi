namespace MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Representa um resultado de pesquisa paginada para a API de módulos.
/// </summary>
/// <param name="Items">Lista de ModuleSearchableProviderDto retornados na página atual.</param>
/// <param name="TotalCount">Número total de itens correspondentes à pesquisa.</param>
/// <param name="PageNumber">Número da página atual.</param>
/// <param name="PageSize">Tamanho da página (quantidade de itens por página).</param>
public sealed record ModulePagedSearchResultDto(
    IReadOnlyList<ModuleSearchableProviderDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

