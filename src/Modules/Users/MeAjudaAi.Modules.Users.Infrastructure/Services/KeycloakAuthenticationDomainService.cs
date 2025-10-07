using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services;

internal class KeycloakAuthenticationDomainService(IKeycloakService keycloakService) : IAuthenticationDomainService
{
    public async Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await keycloakService.AuthenticateAsync(usernameOrEmail, password, cancellationToken);
    }

    public async Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await keycloakService.ValidateTokenAsync(token, cancellationToken);
    }
}