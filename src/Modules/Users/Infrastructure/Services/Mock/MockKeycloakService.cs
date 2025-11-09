using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

/// <summary>
/// 🧪 MOCK DO SERVIÇO KEYCLOAK PARA TESTES
/// 
/// Implementação mock simples para uso quando Keycloak está desabilitado.
/// Retorna respostas válidas sem fazer chamadas reais para o Keycloak.
/// </summary>
internal sealed class MockKeycloakService : IKeycloakService
{
    public Task<Result<string>> CreateUserAsync(string username, string email, string firstName, string lastName, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var keycloakId = $"keycloak-{Guid.NewGuid()}";
        return Task.FromResult(Result<string>.Success(keycloakId));
    }

    public Task<Result<AuthenticationResult>> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var result = new AuthenticationResult(
            UserId: Guid.NewGuid(),
            AccessToken: "mock-access-token",
            RefreshToken: "mock-refresh-token",
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            Roles: new[] { "user" }
        );
        return Task.FromResult(Result<AuthenticationResult>.Success(result));
    }

    public Task<Result<TokenValidationResult>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var result = new TokenValidationResult(
            UserId: Guid.NewGuid(),
            Roles: new[] { "user" },
            Claims: new Dictionary<string, object>
            {
                { "sub", Guid.NewGuid().ToString() },
                { "preferred_username", "mock-user" },
                { "email", "mock@example.com" }
            }
        );
        return Task.FromResult(Result<TokenValidationResult>.Success(result));
    }

    public Task<Result> DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success());
    }
}