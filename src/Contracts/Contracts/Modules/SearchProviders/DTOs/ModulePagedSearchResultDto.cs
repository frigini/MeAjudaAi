namespace MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Paginated search result DTO for module API.
/// </summary>
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

