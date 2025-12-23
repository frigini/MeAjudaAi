using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Domain.Services;

/// <summary>
/// Serviço de domínio para operações de usuário.
/// </summary>
public interface IUserDomainService
{
    /// <param name="phoneNumber">Número de telefone opcional do usuário</param>
    Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default);

    Task<Result> SyncUserWithKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}
