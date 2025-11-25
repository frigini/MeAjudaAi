namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Paginated search result DTO for module API.
/// </summary>
public sealed record ModulePagedSearchResultDto
{
    public required IReadOnlyList<ModuleSearchableProviderDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
