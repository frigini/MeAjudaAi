using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Contracts;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; } 
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }

    [JsonConstructor]
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount, int totalPages, bool hasNextPage, bool hasPreviousPage)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
    }

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        => new(items, page, pageSize, totalCount);
}