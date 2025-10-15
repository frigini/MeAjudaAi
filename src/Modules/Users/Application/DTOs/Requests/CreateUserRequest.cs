using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record CreateUserRequest : Request
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public IEnumerable<string>? Roles { get; init; }
}