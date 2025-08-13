using System.Security.Claims;

namespace MeAjudaAi.Modules.Users.Application.Interfaces;

public interface ITokenValidationService
{
    Task<ClaimsPrincipal?> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<string?> GetUserIdFromTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetUserRolesFromTokenAsync(
        string token,
        CancellationToken cancellationToken = default);
}