using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public interface IKeycloakService
{
    Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> DeactivateUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default);
}