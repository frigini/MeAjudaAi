namespace MeAjudaAi.Modules.Users.Application.DTOs;

public record UserProfileDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}
