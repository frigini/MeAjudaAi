using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.Mock;

/// <summary>
/// 🧪 MOCK DO SERVIÇO DE DOMÍNIO DE USUÁRIOS PARA TESTES
/// 
/// Implementação mock simples para uso quando Keycloak está desabilitado.
/// Cria usuários mock válidos sem fazer chamadas reais para o Keycloak.
/// </summary>
internal sealed class MockUserDomainService : IUserDomainService
{
    public Task<Result<User>> CreateUserAsync(Username username, Email email, string firstName, string lastName, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var user = new User(
            username,
            email,
            firstName,
            lastName,
            $"mock-keycloak-{Guid.NewGuid()}"
        );
        
        return Task.FromResult(Result<User>.Success(user));
    }

    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Mock sempre sincroniza com sucesso
        return Task.FromResult(Result.Success());
    }
}