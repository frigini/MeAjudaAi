using MeAjudaAi.Modules.Users.Application.DTOs.Responses;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public interface IKeycloakService
{
    Task<Result<KeycloakTokenResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<KeycloakUserResponse>> CreateUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<KeycloakTokenResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<KeycloakUserResponse>> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateUserAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> AssignRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
}