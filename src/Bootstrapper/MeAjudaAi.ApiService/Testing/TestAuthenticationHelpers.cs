using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.ApiService.Testing;

/// <summary>
/// Opções para o esquema de autenticação de teste.
/// NOTA: Esta classe existe apenas para suportar testes de integração.
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Usuário padrão para testes
    /// </summary>
    public string DefaultUserId { get; set; } = "test-user-id";

    /// <summary>
    /// Nome do usuário padrão para testes
    /// </summary>
    public string DefaultUserName { get; set; } = "test-user";
}

/// <summary>
/// Handler de autenticação simplificado para ambiente de teste.
/// NOTA: Esta classe existe apenas para suportar testes de integração.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        ILoggerFactory logger, 
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Para testes, sempre autenticamos com um usuário padrão
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "user")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
