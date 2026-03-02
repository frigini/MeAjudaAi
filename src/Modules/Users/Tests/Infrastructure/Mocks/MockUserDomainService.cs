using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;

public class MockUserDomainService : IUserDomainService
{
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
        // Para testes, criar usuário mock
        var userResult = User.Create(username, email, firstName, lastName, Guid.NewGuid().ToString(), phoneNumber);
        return Task.FromResult(Result<User>.Success(userResult.Value));
    }

    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para testes, simular sincronização bem-sucedida
        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeactivateUserInKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para testes, simular desativação bem-sucedida
        return Task.FromResult(Result.Success());
    }
}
