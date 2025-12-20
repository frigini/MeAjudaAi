using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para classes de teste facilitar configuração de usuários
/// </summary>
public static class TestBaseAuthExtensions
{
    /// <summary>
    /// Configura um usuário administrador para o teste
    /// </summary>
    public static void AuthenticateAsAdmin(this object testBase,
        string userId = "admin-id",
        string username = "admin",
        string email = "admin@test.com")
    {
        ConfigurableTestAuthenticationHandler.ConfigureAdmin(userId, username, email);
    }

    /// <summary>
    /// Configura um usuário normal para o teste
    /// </summary>
    public static void AuthenticateAsUser(this object testBase,
        string userId = "user-id",
        string username = "user",
        string email = "user@test.com")
    {
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username, email);
    }

    /// <summary>
    /// Configura usuário customizado para o teste
    /// </summary>
    public static void AuthenticateAsCustomUser(this object testBase,
        string userId,
        string username,
        string email,
        params string[] roles)
    {
        ConfigurableTestAuthenticationHandler.ConfigureUser(userId, username, email, Array.Empty<string>(), false, roles);
    }

    /// <summary>
    /// Remove a autenticação (usuário anônimo)
    /// </summary>
    public static void AuthenticateAsAnonymous(this object testBase)
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }
}
