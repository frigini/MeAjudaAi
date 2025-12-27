using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;

/// <summary>
/// Implementação para desenvolvimento local de IUserDomainService para ambientes onde o Keycloak não está disponível.
/// Este serviço cria usuários localmente sem integração com autenticação externa.
/// Usado apenas para desenvolvimento local quando o Keycloak está desabilitado na configuração.
/// </summary>
internal class LocalDevelopmentUserDomainService : IUserDomainService
{
    /// <summary>
    /// Cria um usuário localmente sem integração com Keycloak.
    /// Gera um ID mock do Keycloak usando UUID v7 para ordenação baseada em tempo.
    /// </summary>
    public Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        // Para ambientes sem Keycloak, criar usuário mock com ID simulado
        // Using UuidGenerator.NewId() for better time-based ordering and performance
        var user = new User(username, email, firstName, lastName, $"mock_keycloak_{UuidGenerator.NewId()}", phoneNumber);
        return Task.FromResult(Result<User>.Success(user));
    }

    /// <summary>
    /// Simula sincronização com Keycloak.
    /// Sempre retorna sucesso para a implementação mock.
    /// </summary>
    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para ambientes sem Keycloak, simular sincronização bem-sucedida
        return Task.FromResult(Result.Success());
    }
}
