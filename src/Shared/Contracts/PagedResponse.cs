using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Contracts;

public record PagedResponse<TData> : Response<TData>
{
    [JsonConstructor]
    public PagedResponse(
        TData? data,
        int totalCount,
        int currentPage = 1,
        int pageSize = 15)
        : base(data)
    {
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }

    public PagedResponse(
        TData? data,
        int statusCode = 15,
        string? message = null)
        : base(data, statusCode, message)
    {
    }

    public int CurrentPage { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public int PageSize { get; init; } = 15;
    public int TotalCount { get; init; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}