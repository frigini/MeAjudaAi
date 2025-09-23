using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Auth;

/// <summary>
/// Extensões para facilitar a configuração de autenticação nos testes
/// </summary>
public static class ApiTestBaseAuthExtensions
{
    /// <summary>
    /// Configura um usuário administrador para o teste
    /// </summary>
    public static void AuthenticateAsAdmin(this ApiTestBase testBase, 
        string userId = "admin-id", 
        string username = "admin", 
        string email = "admin@test.com")
    {
        FakeAuthenticationHandler.SetAdminUser(userId, username, email);
    }

    /// <summary>
    /// Configura um usuário normal para o teste
    /// </summary>
    public static void AuthenticateAsUser(this ApiTestBase testBase, 
        string userId = "user-id", 
        string username = "user", 
        string email = "user@test.com")
    {
        FakeAuthenticationHandler.SetRegularUser(userId, username, email);
    }

    /// <summary>
    /// Configura um usuário customizado para o teste
    /// </summary>
    public static void AuthenticateAs(this ApiTestBase testBase, 
        string userId, 
        string username, 
        string email, 
        params string[] roles)
    {
        FakeAuthenticationHandler.SetTestUser(userId, username, email, roles);
    }

    /// <summary>
    /// Remove a autenticação (usuário anônimo)
    /// </summary>
    public static void AuthenticateAsAnonymous(this ApiTestBase testBase)
    {
        FakeAuthenticationHandler.ClearTestUser();
    }
}