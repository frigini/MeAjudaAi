namespace MeAjudaAi.Modules.Users.Domain.Services.Models;

public sealed record AuthenticationResult(
    Guid? UserId = null,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    IEnumerable<string>? Roles = null
);