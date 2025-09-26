using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;

internal class MockAuthenticationDomainService : IAuthenticationDomainService
{
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
        // Para testes, validar tokens que começam com "mock_token_"
        if (token.StartsWith("mock_token_"))
        {
            var result = new TokenValidationResult(
                UserId: Guid.NewGuid(),
                Roles: ["customer"],
                Claims: new Dictionary<string, object> { ["sub"] = Guid.NewGuid().ToString() }
            );
            return Task.FromResult(Result<TokenValidationResult>.Success(result));
        }
        
        var invalidResult = new TokenValidationResult(
            UserId: null,
            Roles: [],
            Claims: []
        );
        return Task.FromResult(Result<TokenValidationResult>.Success(invalidResult));
    }
}
