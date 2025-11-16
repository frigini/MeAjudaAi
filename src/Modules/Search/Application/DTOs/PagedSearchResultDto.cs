namespace MeAjudaAi.Modules.Search.Application.DTOs;

/// <summary>
/// DTO representing paginated search results.
/// </summary>
/// <typeparam name="T">Type of items in the result set</typeparam>
public sealed record PagedSearchResultDto<T>
{
    /// <summary>
    /// List of items in the current page.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Total number of items matching the search criteria.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there are more pages available.
    /// </summary>
    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
