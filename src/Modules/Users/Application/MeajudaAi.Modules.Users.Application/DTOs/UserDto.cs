namespace MeAjudaAi.Modules.Users.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Status,
    string KeycloakId,
    List<string> Roles,
    DateTime? LastLoginAt,
    bool IsServiceProvider,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public string FullName => $"{FirstName} {LastName}";
}