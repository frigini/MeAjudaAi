using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Infrastructure.Services;

public class KeycloakUserDomainService(IKeycloakService keycloakService) : IUserDomainService
{
    public async Task<Result<User>> CreateUserAsync(
        Username username,
        Email email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var keycloakResult = await keycloakService.CreateUserAsync(
            username.Value, email.Value, firstName, lastName, password, roles, cancellationToken);

        if (keycloakResult.IsFailure)
            return Result<User>.Failure(keycloakResult.Error);

        var user = new User(username, email, firstName, lastName, keycloakResult.Value);
        return Result<User>.Success(user);
    }

    public async Task<Result> SyncUserWithKeycloakAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        // Implementation for syncing user data with Keycloak
        // This could involve deactivating user, updating roles, etc.
        await Task.CompletedTask;
        return Result.Success();
    }
}