using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Users;

/// <summary>
/// Mock de IKeycloakService para testes de integração e E2E.
/// Simula operações do Keycloak sem dependência de infraestrutura externa.
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
        var keycloakId = Guid.NewGuid().ToString();
        return Task.FromResult(Result<string>.Success(keycloakId));
    }

    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
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
        return Task.FromResult(Result.Success());
    }
}
