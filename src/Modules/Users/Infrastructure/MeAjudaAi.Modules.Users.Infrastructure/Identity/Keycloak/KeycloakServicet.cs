using MeAjudaAi.Modules.Users.Application.DTOs.Responses;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public class KeycloakServicet(
    HttpClient httpClient,
    IOptions<KeycloakOptions> config,
    ILogger<KeycloakServicet> logger) : IKeycloakService
{
    private readonly KeycloakOptions _config = config.Value;
    private string? _adminToken;
    private DateTime _adminTokenExpiry;

    public async Task<Result<KeycloakTokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["username"] = email,
            ["password"] = password
        };

        try
        {
            var response = await httpClient.PostAsync(
                $"{_config.BaseUrl}/realms/{_config.Realm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Keycloak login failed: {Error}", error);
                return Result<KeycloakTokenResponse>.Failure("Invalid credentials");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
            return Result<KeycloakTokenResponse>.Success(tokenResponse!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Keycloak login for user {Email}", email);
            return Result<KeycloakTokenResponse>.Failure("Login failed");
        }
    }

    public async Task<Result<KeycloakUserResponse>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAdminTokenAsync(cancellationToken);

            var userRequest = new
            {
                username = email,
                email,
                firstName,
                lastName,
                enabled = true,
                emailVerified = false,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);

            var response = await httpClient.PostAsJsonAsync(
                $"{_config.BaseUrl}/admin/realms/{_config.Realm}/users",
                userRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to create user in Keycloak: {Error}", error);
                return Result<KeycloakUserResponse>.Failure("Failed to create user");
            }

            // Extrair ID do usuário do header Location
            var location = response.Headers.Location?.ToString();
            var userId = location?.Split('/').LastOrDefault();

            return Result<KeycloakUserResponse>.Success(new KeycloakUserResponse
            {
                UserId = userId ?? Guid.NewGuid().ToString(),
                Username = email,
                Email = email,
                EmailVerified = false,
                Enabled = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user in Keycloak");
            return Result<KeycloakUserResponse>.Failure("Failed to create user");
        }
    }

    public async Task<Result<KeycloakTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        try
        {
            var response = await httpClient.PostAsync(
                $"{_config.BaseUrl}/realms/{_config.Realm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<KeycloakTokenResponse>.Failure("Invalid refresh token");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
            return Result<KeycloakTokenResponse>.Success(tokenResponse!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Result<KeycloakTokenResponse>.Failure("Token refresh failed");
        }
    }

    public async Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var logoutRequest = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        try
        {
            var response = await httpClient.PostAsync(
                $"{_config.BaseUrl}/realms/{_config.Realm}/protocol/openid-connect/logout",
                new FormUrlEncodedContent(logoutRequest),
                cancellationToken);

            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Result<bool>.Failure("Logout failed");
        }
    }

    private async Task EnsureAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (_adminToken != null && DateTime.UtcNow < _adminTokenExpiry)
            return;

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _config.AdminUsername,
            ["password"] = _config.AdminPassword
        };

        var response = await httpClient.PostAsync(
            $"{_config.BaseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
        _adminToken = tokenResponse!.AccessToken;
        _adminTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 1 min buffer
    }

    Task<Result<KeycloakUserResponse>> IKeycloakService.GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<bool>> IKeycloakService.UpdateUserAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<bool>> IKeycloakService.DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<bool>> IKeycloakService.AssignRoleAsync(string userId, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}