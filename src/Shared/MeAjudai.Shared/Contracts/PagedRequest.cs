namespace MeAjudaAi.Shared.Contracts;

public abstract record PagedRequest : Request
{
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
}