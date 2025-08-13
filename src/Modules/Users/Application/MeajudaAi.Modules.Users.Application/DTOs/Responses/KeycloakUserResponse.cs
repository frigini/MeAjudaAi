namespace MeAjudaAi.Modules.Users.Application.DTOs.Responses;

public record KeycloakUserResponse
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public bool Enabled { get; init; }
}