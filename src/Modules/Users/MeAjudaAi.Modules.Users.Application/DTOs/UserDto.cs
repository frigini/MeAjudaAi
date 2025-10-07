namespace MeAjudaAi.Modules.Users.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string KeycloakId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);