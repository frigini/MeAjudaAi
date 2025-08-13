using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record GetUsersRequest : PagedRequest
{
    public string? Email { get; init; }
    public string? Role { get; init; }
    public string? Status { get; init; }
}