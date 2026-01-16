using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Users.Domain.Services;

/// <summary>
/// Interface do serviço de domínio para operações de autenticação.
/// </summary>
public interface IAuthenticationDomainService
{
    Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);
}
