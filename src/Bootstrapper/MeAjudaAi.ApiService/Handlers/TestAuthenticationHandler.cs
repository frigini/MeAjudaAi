using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MeAjudaAi.ApiService.Handlers;

/// <summary>
/// ⚠️ TESTING AUTHENTICATION HANDLER - DEVELOPMENT/TESTING ENVIRONMENTS ONLY ⚠️
/// 
/// Handler que SEMPRE retorna sucesso com claims de administrador para testes automatizados.
/// 
/// 🚨 NUNCA USE EM PRODUÇÃO! 🚨
/// 
/// Para documentação completa, veja: /docs/testing/test-authentication-handler.md
/// </summary>
/// <remarks>
/// Este handler bypassa completamente a autenticação real e é usado exclusivamente em:
/// - Desenvolvimento local (Development)
/// - Testes de integração (Testing)
/// - Pipelines CI/CD
/// 
/// Documentação detalhada disponível em:
/// - Configuração: /docs/testing/test-auth-configuration.md
/// - Exemplos: /docs/testing/test-auth-examples.md
/// </remarks>
/// <remarks>
/// Inicializa uma nova instância do TestAuthenticationHandler.
/// </remarks>
/// <param name="options">Opções de configuração do esquema de autenticação</param>
/// <param name="logger">Logger para registrar atividades de autenticação</param>
/// <param name="encoder">Encoder de URL para processamento de parâmetros</param>
public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ILogger<TestAuthenticationHandler> _logger = logger.CreateLogger<TestAuthenticationHandler>();

    /// <summary>
    /// Processa a autenticação da requisição sempre retornando sucesso com claims de admin.
    /// 
    /// Para detalhes sobre claims gerados e comportamento, veja:
    /// /docs/testing/test-auth-configuration.md
    /// </summary>
    /// <returns>
    /// Sempre retorna AuthenticateResult.Success com claims de administrador.
    /// </returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Log de segurança para auditoria em ambientes de teste
        _logger.LogWarning(
            "🚨 TEST AUTHENTICATION ACTIVE: Bypassing real authentication. " +
            "Request from {RemoteIpAddress} authenticated as admin user automatically. " +
            "Ensure this is NOT a production environment!",
            Context.Connection.RemoteIpAddress);

        // Criação de claims fixos para usuário de teste com privilégios administrativos
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id", ClaimValueTypes.String),
            new Claim("sub", "test-user-id", ClaimValueTypes.String), // Subject claim padrão JWT
            new Claim(ClaimTypes.Name, "test-user", ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, "test@example.com", ClaimValueTypes.Email),
            new Claim(ClaimTypes.Role, "admin", ClaimValueTypes.String),
            new Claim("roles", "admin", ClaimValueTypes.String), // Para múltiplos papéis se necessário
            new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer), // Issued at
            new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer) // Expires
        };

        // Criação da identidade autenticada com esquema de teste
        var identity = new ClaimsIdentity(claims, "AspireTest", ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "AspireTest");
        
        // Log detalhado para debugging de testes
        _logger.LogDebug(
            "Test authentication completed. Generated claims: {ClaimsCount}, " +
            "Identity: {IdentityName}, IsAuthenticated: {IsAuthenticated}",
            claims.Length, identity.Name, identity.IsAuthenticated);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}