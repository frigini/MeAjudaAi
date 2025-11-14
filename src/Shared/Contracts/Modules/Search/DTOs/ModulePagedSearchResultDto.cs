namespace MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;

/// <summary>
/// Paginated search result DTO for module API.
/// </summary>
public sealed record ModulePagedSearchResultDto
{
    public required IReadOnlyList<ModuleSearchableProviderDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
