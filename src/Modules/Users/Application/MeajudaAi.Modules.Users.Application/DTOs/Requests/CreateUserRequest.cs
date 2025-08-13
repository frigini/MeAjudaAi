using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record CreateUserRequest : Request
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = "Customer";
    public bool EmailVerified { get; init; } = false;
}