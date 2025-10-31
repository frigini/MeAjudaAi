using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;

/// <summary>
/// Implementações mock específicas para testes do módulo Users
/// </summary>
public class MockKeycloakService : IKeycloakService
{
    public Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        // Para testes, simular criação bem-sucedida
        var keycloakId = $"keycloak_{Guid.NewGuid()}";
        return Task.FromResult(Result<string>.Success(keycloakId));
    }

    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Para testes, validar apenas credenciais específicas
        if (usernameOrEmail == "validuser" && password == "validpassword")
        {
            var result = new AuthenticationResult(
                UserId: Guid.NewGuid(),
                AccessToken: $"mock_token_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                RefreshToken: $"mock_refresh_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                ExpiresAt: DateTime.UtcNow.AddHours(1),
                Roles: ["customer"]
            );
            return Task.FromResult(Result<AuthenticationResult>.Success(result));
        }

        return Task.FromResult(Result<AuthenticationResult>.Failure("Invalid credentials"));
    }

    public Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        // Para testes, validar tokens que começam com "mock_token_"
        if (token.StartsWith("mock_token_", StringComparison.OrdinalIgnoreCase))
        {
            var result = new TokenValidationResult(
                UserId: Guid.NewGuid(),
                Roles: ["customer"],
                Claims: new Dictionary<string, object> { ["sub"] = Guid.NewGuid().ToString() }
            );
            return Task.FromResult(Result<TokenValidationResult>.Success(result));
        }

        return Task.FromResult(Result<TokenValidationResult>.Failure("Invalid token"));
    }

    public Task<Result> DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        // Para testes, simular desativação bem-sucedida
        return Task.FromResult(Result.Success());
    }
}
