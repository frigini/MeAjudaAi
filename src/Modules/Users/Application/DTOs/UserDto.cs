using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.DTOs;

[ExcludeFromCodeCoverage]

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? DeviceToken,
    string? PhoneNumber,
    bool IsActive,
    string KeycloakId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
