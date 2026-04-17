namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

/// <summary>
/// Implementação de configuração de autenticação para testes com estado baseado em instância
/// </summary>
public class TestAuthenticationConfiguration : ITestAuthenticationConfiguration
{
    public string? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public Guid? ProviderId { get; private set; }
    public IReadOnlyList<string>? Roles { get; private set; }
    public bool HasUser => UserId != null;
    public bool AllowUnauthenticated { get; private set; }

    public void ConfigureUser(string userId, string userName, string email, params string[] roles)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        Roles = roles;
        ProviderId = null;
    }

    public void ConfigureAdmin(string userId = "admin-id", string userName = "admin", string email = "admin@test.com")
    {
        ConfigureUser(userId, userName, email, "admin");
    }

    public void ConfigureRegularUser(string userId = "user-id", string userName = "user", string email = "user@test.com")
    {
        ConfigureUser(userId, userName, email, "user");
    }

    public void ConfigureProvider(Guid providerId, string userId = "provider-id", string userName = "provider", string email = "provider@test.com")
    {
        ConfigureUser(userId, userName, email, "provider");
        ProviderId = providerId;
    }

    public void ClearConfiguration()
    {
        UserId = null;
        UserName = null;
        Email = null;
        Roles = null;
        ProviderId = null;
        AllowUnauthenticated = false;
    }

    public void SetAllowUnauthenticated(bool allow)
    {
        AllowUnauthenticated = allow;
    }
}
