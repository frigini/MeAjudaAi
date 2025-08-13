using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.DTOs;

public record LoginRequest : Request
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}