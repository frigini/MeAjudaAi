using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

/// <summary>
/// 🧪 HELPER PARA CRIAÇÃO DE DADOS MOCK DE AUTENTICAÇÃO DETERMINÍSTICOS
/// 
/// Centraliza a lógica de criação de dados mock para testes, garantindo
/// valores determinísticos e evitando duplicação entre os serviços mock.
/// </summary>
internal static class MockAuthenticationHelper
{
    // Fixed deterministic values for consistent testing
    private static readonly Guid FixedUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    private static readonly DateTime FixedExpirationTime = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
    private static int _keycloakIdCounter = 0;

    public static AuthenticationResult CreateMockAuthenticationResult(string[]? roles = null)
    {
        return new AuthenticationResult(
            UserId: FixedUserId,
            AccessToken: "mock-access-token",
            RefreshToken: "mock-refresh-token",
            ExpiresAt: FixedExpirationTime,
            Roles: roles ?? new[] { "user" }
        );
    }

    public static TokenValidationResult CreateMockTokenValidationResult(string[]? roles = null)
    {
        return new TokenValidationResult(
            UserId: FixedUserId,
            Roles: roles ?? new[] { "user" },
            Claims: new Dictionary<string, object>
            {
                { AuthConstants.Claims.Subject, FixedUserId.ToString() },
                { AuthConstants.Claims.PreferredUsername, "mock-user" },
                { AuthConstants.Claims.Email, "mock@example.com" }
            }
        );
    }

    public static string CreateMockKeycloakId(string? userSpecificValue = null)
    {
        if (!string.IsNullOrEmpty(userSpecificValue))
        {
            return $"keycloak-{userSpecificValue}";
        }

        // Generate unique ID using counter for thread safety
        var uniqueId = Interlocked.Increment(ref _keycloakIdCounter);
        return $"keycloak-{FixedUserId}-{uniqueId}";
    }
}
