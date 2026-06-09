using MeAjudaAi.Contracts.Utilities.Constants;

namespace Meajudai.Contracts.Models;

public abstract record PagedRequest
{
    public int PageSize { get; init; } = Pagination.DefaultPageSize;
    public int PageNumber { get; init; } = Pagination.DefaultPageNumber;
}