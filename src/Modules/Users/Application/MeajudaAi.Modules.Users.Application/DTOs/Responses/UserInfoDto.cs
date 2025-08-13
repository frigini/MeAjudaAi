namespace MeAjudaAi.Modules.Users.Application.DTOs.Responses;

public record UserInfoDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
    public Dictionary<string, object> Claims { get; init; } = [];
}