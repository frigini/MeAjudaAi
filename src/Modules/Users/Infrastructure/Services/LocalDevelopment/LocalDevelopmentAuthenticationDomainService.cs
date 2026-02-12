using System.Security.Cryptography;
using System.Text;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;

/// <summary>
/// Implementação para desenvolvimento local de IAuthenticationDomainService para ambientes onde o Keycloak não está disponível.
/// Fornece lógica básica de autenticação para cenários de desenvolvimento local.
/// Usado apenas para desenvolvimento local quando o Keycloak está desabilitado na configuração.
/// </summary>
internal class LocalDevelopmentAuthenticationDomainService : IAuthenticationDomainService
{
    /// <summary>
    /// Autentica usuários com credenciais mock para desenvolvimento local.
    /// </summary>
    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Para ambientes de desenvolvimento local, aceitar credenciais específicas
        if ((usernameOrEmail == "testuser" || usernameOrEmail == "test@example.com") && password == "testpassword")
        {
            var deterministicUserId = GenerateDeterministicGuid(usernameOrEmail);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var result = new AuthenticationResult(
                UserId: deterministicUserId,
                AccessToken: $"mock_token_{deterministicUserId}_{timestamp}",
                RefreshToken: $"mock_refresh_{deterministicUserId}_{timestamp}",
                ExpiresAt: DateTime.UtcNow.AddHours(1),
                Roles: ["customer"]
            );
            return Task.FromResult(Result<AuthenticationResult>.Success(result));
        }

        return Task.FromResult(Result<AuthenticationResult>.Failure("Invalid credentials"));
    }

    /// <summary>
    /// Valida tokens mock para desenvolvimento local.
    /// </summary>
    public Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // Para ambientes de desenvolvimento local, validar tokens que começam com "mock_token_"
        if (token.StartsWith("mock_token_"))
        {
            // Extrair o userId do token: "mock_token_{userId}_{timestamp}"
            var parts = token.Split('_');
            Guid userId;

            if (parts.Length >= 3 && Guid.TryParse(parts[2], out userId))
            {
                // Use o userId extraído do token
                var result = new TokenValidationResult(
                    UserId: userId,
                    Roles: ["customer"],
                    Claims: new Dictionary<string, object> { ["sub"] = userId.ToString() }
                );
                return Task.FromResult(Result<TokenValidationResult>.Success(result));
            }
            else
            {
                // Fallback determinístico se não conseguir extrair o userId
                var fallbackUserId = GenerateDeterministicGuid("fallback");
                var result = new TokenValidationResult(
                    UserId: fallbackUserId,
                    Roles: ["customer"],
                    Claims: new Dictionary<string, object> { ["sub"] = fallbackUserId.ToString() }
                );
                return Task.FromResult(Result<TokenValidationResult>.Success(result));
            }
        }

        return Task.FromResult(Result<TokenValidationResult>.Failure("Invalid token"));
    }

    /// <summary>
    /// Generates a deterministic GUID based on the input string.
    /// Same input will always produce the same GUID.
    /// </summary>
    private static Guid GenerateDeterministicGuid(string input)
    {
        // Normalize the input to lowercase for consistency
        var normalizedInput = input.ToLowerInvariant();

        // Generate SHA256 hash of the normalized input (and take first 16 bytes for GUID)
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedInput));

        // Use the first 16 bytes of the hash to create a GUID
        return new Guid(hash.AsSpan(0, 16));
    }
}
