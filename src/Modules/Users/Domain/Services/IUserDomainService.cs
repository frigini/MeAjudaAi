using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Domain.Services;

/// <summary>
/// Serviço de domínio para operações de usuário.
/// </summary>
public interface IUserDomainService
{
    /// <summary>
    /// Cria um novo usuário no sistema.
    /// </summary>
    /// <param name="username">Nome de usuário único</param>
    /// <param name="email">Endereço de email único</param>
    /// <param name="firstName">Primeiro nome</param>
    /// <param name="lastName">Sobrenome</param>
    /// <param name="password">Senha</param>
    /// <param name="roles">Papéis/roles do usuário</param>
    /// <param name="phoneNumber">Número de telefone opcional do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado com o usuário criado ou erro</returns>
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
