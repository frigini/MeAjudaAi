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
    /// Configura um usuário prestador para o teste
    /// </summary>
    public static void AuthenticateAsProvider(this object testBase,
        Guid providerId,
        string userId = "provider-id",
        string username = "provider",
        string email = "provider@test.com")
    {
        ConfigurableTestAuthenticationHandler.ConfigureProvider(providerId, userId, username, email);
    }

    /// <summary>
    /// Configura um usuário prestador para o teste usando autenticação baseada em instância
    /// </summary>
    public static void AuthenticateAsProviderInstance(this object testBase,
        Guid providerId,
        ITestAuthenticationConfiguration? authConfig = null,
        string userId = "provider-id",
        string username = "provider",
        string email = "provider@test.com")
    {
        if (authConfig != null)
        {
            authConfig.ConfigureProvider(providerId, userId, username, email);
            return;
        }
        
        if (testBase is ITestAuthenticationConfiguration config)
        {
            config.ConfigureProvider(providerId, userId, username, email);
            return;
        }

        var bindingFlags = System.Reflection.BindingFlags.Instance | 
                           System.Reflection.BindingFlags.Public | 
                           System.Reflection.BindingFlags.NonPublic;

        var prop = testBase.GetType().GetProperty("AuthConfig", bindingFlags) ?? 
                 testBase.GetType().GetProperty("Auth", bindingFlags);
        var configValue = prop?.GetValue(testBase);
        if (configValue is ITestAuthenticationConfiguration instanceConfig)
        {
            instanceConfig.ConfigureProvider(providerId, userId, username, email);
            return;
        }

        throw new InvalidOperationException($"Authentication configuration missing for test '{testBase.GetType().FullName}'. ConfigureProvider could not be applied.");
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
