using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

/// <summary>
/// 🧪 MOCK DO SERVIÇO DE AUTENTICAÇÃO PARA TESTES
/// 
/// Implementação mock simples para uso quando Keycloak está desabilitado.
/// Retorna respostas válidas e determinísticas usando MockAuthenticationHelper.
/// </summary>
internal sealed class MockAuthenticationDomainService : IAuthenticationDomainService
{
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
}
