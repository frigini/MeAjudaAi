using System.Collections.Concurrent;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

/// <summary>
/// 🧪 MOCK DO SERVIÇO KEYCLOAK PARA TESTES
/// 
/// Implementação mock simples para uso quando Keycloak está desabilitado.
/// Retorna respostas válidas e determinísticas usando MockAuthenticationHelper.
/// </summary>
internal sealed class MockKeycloakService : IKeycloakService
{
    // Thread-safe in-memory storage for roles per keycloak user (for testing purposes)
    private static readonly ConcurrentDictionary<string, string[]> _userRoles = new();

    public Task<Result<string>> CreateUserAsync(string username, string email, string firstName, string lastName, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var keycloakId = MockAuthenticationHelper.CreateMockKeycloakId();

        // Store roles for this mock user for potential future validation
        if (roles != null)
        {
            _userRoles.TryAdd(keycloakId, roles.ToArray());
        }

        return Task.FromResult(Result<string>.Success(keycloakId));
    }

    public Task<Result<AuthenticationResult>> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var result = MockAuthenticationHelper.CreateMockAuthenticationResult();
        return Task.FromResult(Result<AuthenticationResult>.Success(result));
    }

    public Task<Result<TokenValidationResult>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var result = MockAuthenticationHelper.CreateMockTokenValidationResult();
        return Task.FromResult(Result<TokenValidationResult>.Success(result));
    }

    public Task<Result> DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        // Remove from our mock storage when deactivated using thread-safe method
        _userRoles.TryRemove(keycloakId, out _);
        return Task.FromResult(Result.Success());
    }
}
