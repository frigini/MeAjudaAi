using MeAjudaAi.Modules.Users.Application.DTOs.Responses;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Services;

public class KeycloakService : IKeycloakService
{
    private readonly ICacheService _cache;
    private readonly ILogger<KeycloakService> _logger;

    public KeycloakService(
        ICacheService cache,
        ILogger<KeycloakService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<Result<bool>> AssignRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<KeycloakUserResponse>> CreateUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<KeycloakUserResponse>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<KeycloakTokenResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<KeycloakTokenResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> UpdateUserAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}