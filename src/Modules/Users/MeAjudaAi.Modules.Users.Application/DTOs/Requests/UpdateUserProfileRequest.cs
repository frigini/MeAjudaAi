using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record UpdateUserProfileRequest : Request
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}