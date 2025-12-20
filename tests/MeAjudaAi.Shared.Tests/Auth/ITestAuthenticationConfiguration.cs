namespace MeAjudaAi.Shared.Tests.Auth;

/// <summary>
/// Interface para configuração de autenticação em testes com estado baseado em instância
/// </summary>
public interface ITestAuthenticationConfiguration
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyList<string>? Roles { get; }
    bool HasUser { get; }
    bool AllowUnauthenticated { get; }

    void ConfigureUser(string userId, string userName, string email, params string[] roles);
    void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com");
    void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com");
    void ClearConfiguration();
    void SetAllowUnauthenticated(bool allow);
}
