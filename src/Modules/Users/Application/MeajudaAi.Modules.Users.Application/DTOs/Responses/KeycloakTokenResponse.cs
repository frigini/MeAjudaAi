namespace MeAjudaAi.Modules.Users.Application.DTOs.Responses;

public record KeycloakTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public string Scope { get; init; } = string.Empty;
}