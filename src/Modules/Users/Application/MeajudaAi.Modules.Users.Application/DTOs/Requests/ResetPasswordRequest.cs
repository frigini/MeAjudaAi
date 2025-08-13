using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

public record ResetPasswordRequest : Request
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}