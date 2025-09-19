using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MeAjudaAi.Integration.Tests.Auth;

/// <summary>
/// Authentication handler para testes que permite configurar usuários fake com claims específicas
/// </summary>
public class FakeAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    
    private static readonly List<Claim> _claims = [];

    public FakeAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) 
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_claims.Count == 0)
        {
            // Se não há claims configuradas, retorna falha de autenticação
            return Task.FromResult(AuthenticateResult.Fail("No test user configured"));
        }

        var identity = new ClaimsIdentity(_claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Configura o usuário de teste com claims específicas
    /// </summary>
    public static void SetTestUser(string userId, string username, string email, params string[] roles)
    {
        _claims.Clear();
        _claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        _claims.Add(new Claim("sub", userId)); // Keycloak style claim
        _claims.Add(new Claim(ClaimTypes.Name, username));
        _claims.Add(new Claim(ClaimTypes.Email, email));
        
        foreach (var role in roles)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
            _claims.Add(new Claim("roles", role.ToLowerInvariant())); // Keycloak style claim
        }
    }

    /// <summary>
    /// Configura um usuário administrador para testes
    /// </summary>
    public static void SetAdminUser(string userId = "admin-id", string username = "admin", string email = "admin@test.com")
    {
        SetTestUser(userId, username, email, "admin");
    }

    /// <summary>
    /// Configura um usuário normal para testes
    /// </summary>
    public static void SetRegularUser(string userId = "user-id", string username = "user", string email = "user@test.com")
    {
        SetTestUser(userId, username, email, "user");
    }

    /// <summary>
    /// Remove a autenticação do usuário de teste
    /// </summary>
    public static void ClearTestUser()
    {
        _claims.Clear();
    }
}