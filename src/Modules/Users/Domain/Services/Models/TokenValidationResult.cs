using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Domain.Services.Models;

[ExcludeFromCodeCoverage]
public sealed record TokenValidationResult(
    Guid? UserId = null,
    IEnumerable<string>? Roles = null,
    Dictionary<string, object>? Claims = null
);
