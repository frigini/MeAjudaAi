using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record GetUsersRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
}
