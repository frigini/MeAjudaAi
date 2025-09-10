using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.Services;

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