using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public class KeycloakService(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options,
    ILogger<KeycloakService> logger) : IKeycloakService
{
    private readonly KeycloakOptions _options = options.Value;
    private string? _adminToken;
    private DateTime _adminTokenExpiry = DateTime.MinValue;

    public async Task<Result<string>> CreateUserAsync(
        string username,
        string email,
        string firstName,
        string lastName,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);
            if (adminToken.IsFailure)
                return Result<string>.Failure(adminToken.Error);

            // Create user payload
            var createUserPayload = new KeycloakCreateUserRequest
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Enabled = true,
                EmailVerified = true,
                Credentials =
                [
                    new KeycloakCredential
                    {
                        Type = "password",
                        Value = password,
                        Temporary = false
                    }
                ]
            };

            var json = JsonSerializer.Serialize(createUserPayload, SerializationDefaults.Api);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken.Value}");

            var response = await httpClient.PostAsync(_options.UsersUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to create user in Keycloak: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return Result<string>.Failure($"Failed to create user in Keycloak: {response.StatusCode}");
            }

            // Extract user ID from Location header
            var locationHeader = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(locationHeader))
                return Result<string>.Failure("Failed to get user ID from Keycloak response");

            var keycloakUserId = locationHeader.Split('/').Last();

            // Assign roles if provided
            if (roles.Any())
            {
                var roleAssignResult = await AssignRolesToUserAsync(keycloakUserId, roles, adminToken.Value, cancellationToken);
                if (roleAssignResult.IsFailure)
                {
                    logger.LogWarning("User created but role assignment failed: {Error}", roleAssignResult.Error);
                    // Don't fail user creation, just log the warning
                }
            }

            logger.LogInformation("User created successfully in Keycloak with ID: {UserId}", keycloakUserId);
            return Result<string>.Success(keycloakUserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while creating user in Keycloak. Payload: {Payload}",
                JsonSerializer.Serialize(new { username, email, firstName, lastName }, SerializationDefaults.Logging));
            return Result<string>.Failure($"Exception: {ex.Message}");
        }
    }

    public async Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("client_id", _options.ClientId),
                new("client_secret", _options.ClientSecret),
                new("username", usernameOrEmail),
                new("password", password)
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await httpClient.PostAsync(_options.TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Authentication failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<AuthenticationResult>.Failure("Invalid username/email or password");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
            if (tokenResponse == null)
                return Result<AuthenticationResult>.Failure("Invalid token response from Keycloak");

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(tokenResponse.AccessToken);

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var roles = jwt.Claims.Where(c => c.Type == "realm_access" || c.Type == "resource_access")
                .SelectMany(c => ExtractRolesFromClaim(c.Value))
                .Distinct()
                .ToList();

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Result<AuthenticationResult>.Failure("Invalid user ID in token");

            var authResult = new AuthenticationResult(
                userGuid,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken ?? string.Empty,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                roles
            );

            return Result<AuthenticationResult>.Success(authResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during authentication");
            return Result<AuthenticationResult>.Failure($"Authentication failed: {ex.Message}");
        }
    }

    public async Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return Result<TokenValidationResult>.Failure("Invalid token format");

            var jwt = tokenHandler.ReadJwtToken(token);

            // Check if token is expired
            if (jwt.ValidTo < DateTime.UtcNow)
                return Result<TokenValidationResult>.Failure("Token has expired");

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var roles = jwt.Claims.Where(c => c.Type == "realm_access" || c.Type == "resource_access")
                .SelectMany(c => ExtractRolesFromClaim(c.Value))
                .Distinct()
                .ToList();

            var claims = jwt.Claims.ToDictionary(c => c.Type, c => (object)c.Value);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Result<TokenValidationResult>.Failure("Invalid user ID in token");

            var validationResult = new TokenValidationResult(
                userGuid,
                roles,
                claims
            );

            return Result<TokenValidationResult>.Success(validationResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during token validation");
            return Result<TokenValidationResult>.Failure($"Token validation failed: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateUserAsync(
        string keycloakId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync(cancellationToken);
            if (adminToken.IsFailure)
                return adminToken.Error;

            var updatePayload = new { enabled = false };
            var json = JsonSerializer.Serialize(updatePayload, SerializationDefaults.Api);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken.Value}");

            var response = await httpClient.PutAsync(
                $"{_options.UsersUrl}/{keycloakId}", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to deactivate user in Keycloak: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return Result.Failure($"Failed to deactivate user: {response.StatusCode}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while deactivating user");
            return Result.Failure($"Deactivation failed: {ex.Message}");
        }
    }

    private async Task<Result<string>> GetAdminTokenAsync(CancellationToken cancellationToken = default)
    {
        // Check if we have a valid token
        if (!string.IsNullOrEmpty(_adminToken) && _adminTokenExpiry > DateTime.UtcNow.AddMinutes(5))
            return Result<string>.Success(_adminToken);

        try
        {
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("client_id", _options.ClientId),
                new("client_secret", _options.ClientSecret),
                new("username", _options.AdminUsername),
                new("password", _options.AdminPassword)
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await httpClient.PostAsync(_options.TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to get admin token: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return Result<string>.Failure("Failed to authenticate admin user");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
                SerializationDefaults.Api, cancellationToken);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                return Result<string>.Failure("Invalid admin token response");

            _adminToken = tokenResponse.AccessToken;
            _adminTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return Result<string>.Success(_adminToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while getting admin token");
            return Result<string>.Failure($"Admin token request failed: {ex.Message}");
        }
    }

    private async Task<Result> AssignRolesToUserAsync(
        string keycloakUserId,
        IEnumerable<string> roles,
        string adminToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real scenario, you'd need to:
            // 1. Get available realm roles
            // 2. Map role names to role objects
            // 3. Assign roles to the user

            logger.LogInformation("Role assignment for user {UserId} with roles: {Roles}",
                keycloakUserId, string.Join(", ", roles));

            // Implementation would go here
            await Task.CompletedTask;

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign roles to user {UserId}", keycloakUserId);
            return Result.Failure($"Role assignment failed: {ex.Message}");
        }
    }

    private static IEnumerable<string> ExtractRolesFromClaim(string claimValue)
    {
        try
        {
            // This is a simplified extraction
            // Real implementation would parse the JSON structure properly
            return new List<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }
}