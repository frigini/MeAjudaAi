using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO para criação de novo usuário via API.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CreateUserRequestDto(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IEnumerable<string>? Roles = null,
    string? PhoneNumber = null
);
