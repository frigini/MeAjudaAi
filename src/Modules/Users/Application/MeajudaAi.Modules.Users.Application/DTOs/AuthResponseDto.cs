namespace MeAjudaAi.Modules.Users.Application.DTOs;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public UserDto User { get; init; } = null!;
}