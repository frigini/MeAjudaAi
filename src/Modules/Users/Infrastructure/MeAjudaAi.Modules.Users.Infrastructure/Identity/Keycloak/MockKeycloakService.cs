using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

/// <summary>
/// Implementação mock do serviço Keycloak para testes e desenvolvimento.
/// </summary>
/// <remarks>
/// Implementação que simula o comportamento do Keycloak sem conectar a um servidor real.
/// Utilizada quando Keycloak está desabilitado ou durante testes E2E.
/// Gera IDs únicos simulados e sempre retorna sucesso nas operações.
/// </remarks>
public class MockKeycloakService(ILogger<MockKeycloakService> logger) : IKeycloakService
{
    /// <summary>
    /// Simula a criação de um usuário no Keycloak.
    /// </summary>
    /// <param name="username">Nome de usuário</param>
    /// <param name="email">Email do usuário</param>
    /// <param name="firstName">Primeiro nome</param>
    /// <param name="lastName">Sobrenome</param>
    /// <param name="password">Senha</param>
    /// <param name="roles">Papéis/funções</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>ID simulado do usuário criado</returns>
    /// <remarks>
    /// Gera um GUID único como ID do Keycloak simulado.
    /// Sempre retorna sucesso, não faz validações reais.
    /// </remarks>
    public Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var mockKeycloakId = Guid.NewGuid().ToString();
        
        logger.LogInformation(
            "Mock Keycloak: User {Username} ({Email}) created with simulated ID {KeycloakId}",
            username, email, mockKeycloakId);
        
        return Task.FromResult(Result<string>.Success(mockKeycloakId));
    }

    /// <summary>
    /// Simula autenticação de usuário.
    /// </summary>
    /// <param name="usernameOrEmail">Nome de usuário ou email</param>
    /// <param name="password">Senha</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado de autenticação simulado</returns>
    /// <remarks>
    /// Sempre retorna autenticação bem-sucedida com tokens simulados.
    /// Não valida credenciais reais.
    /// </remarks>
    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var mockUserId = Guid.NewGuid();
        var mockAccessToken = $"mock_access_token_{Guid.NewGuid():N}";
        var mockRefreshToken = $"mock_refresh_token_{Guid.NewGuid():N}";
        var mockExpiry = DateTime.UtcNow.AddHours(1);
        var mockRoles = new List<string> { "user" };

        var authResult = new AuthenticationResult(
            mockUserId,
            mockAccessToken,
            mockRefreshToken,
            mockExpiry,
            mockRoles
        );

        logger.LogInformation(
            "Mock Keycloak: User {Username} authenticated with simulated tokens",
            usernameOrEmail);

        return Task.FromResult(Result<AuthenticationResult>.Success(authResult));
    }

    /// <summary>
    /// Simula validação de token.
    /// </summary>
    /// <param name="token">Token a ser validado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado de validação simulado</returns>
    /// <remarks>
    /// Sempre retorna validação bem-sucedida para qualquer token.
    /// Não faz validação real de JWT ou estrutura.
    /// </remarks>
    public Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var mockUserId = Guid.NewGuid();
        var mockRoles = new List<string> { "user" };
        var mockClaims = new Dictionary<string, object>
        {
            ["sub"] = mockUserId.ToString(),
            ["username"] = "mock_user",
            ["email"] = "mock@example.com"
        };

        var validationResult = new TokenValidationResult(
            mockUserId,
            mockRoles,
            mockClaims
        );

        logger.LogDebug("Mock Keycloak: Token validated with simulated result");

        return Task.FromResult(Result<TokenValidationResult>.Success(validationResult));
    }

    /// <summary>
    /// Simula desativação de usuário.
    /// </summary>
    /// <param name="keycloakId">ID do usuário no Keycloak</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação simulada</returns>
    /// <remarks>
    /// Sempre retorna sucesso na desativação.
    /// Não executa ação real.
    /// </remarks>
    public Task<Result> DeactivateUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Mock Keycloak: User {KeycloakId} deactivated (simulated)",
            keycloakId);

        return Task.FromResult(Result.Success());
    }
}