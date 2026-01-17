using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record GetUsersRequest
{
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
