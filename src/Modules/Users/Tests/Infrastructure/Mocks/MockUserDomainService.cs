using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;

internal class MockUserDomainService : IUserDomainService
{
    public Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        // Para testes, criar usuário mock
        var user = new User(username, email, firstName, lastName, $"keycloak_{Guid.NewGuid()}");
        return Task.FromResult(Result<User>.Success(user));
    }

    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para testes, simular sincronização bem-sucedida
        return Task.FromResult(Result.Success());
    }
}
