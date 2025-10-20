namespace MeAjudaAi.Modules.Users.Domain.Services.Models;

public sealed record TokenValidationResult(
    Guid? UserId = null,
    IEnumerable<string>? Roles = null,
    Dictionary<string, object>? Claims = null
);
