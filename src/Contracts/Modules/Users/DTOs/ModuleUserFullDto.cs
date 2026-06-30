using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO completo de usuário retornado pela API.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ModuleUserFullDto(
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
