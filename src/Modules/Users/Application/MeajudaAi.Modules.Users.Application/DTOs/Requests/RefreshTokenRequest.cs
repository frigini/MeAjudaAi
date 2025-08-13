using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record RefreshTokenRequest : Request
{
    public string RefreshToken { get; init; } = string.Empty;
}