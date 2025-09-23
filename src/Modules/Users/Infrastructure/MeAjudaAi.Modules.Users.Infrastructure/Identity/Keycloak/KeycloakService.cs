using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.Models;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

public class KeycloakService(
    HttpClient httpClient,
    KeycloakOptions options,
    ILogger<KeycloakService> logger) : IKeycloakService
{
    private readonly KeycloakOptions _options = options;
    private string? _adminToken;
    private DateTime _adminTokenExpiry = DateTime.MinValue;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

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

            // Cria payload do usuário
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

            var json = JsonSerializer.Serialize(createUserPayload, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

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

            // Extrai ID do usuário do cabeçalho Location
            var locationHeader = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(locationHeader))
                return Result<string>.Failure("Failed to get user ID from Keycloak response");

            var keycloakUserId = locationHeader.Split('/').Last();

            // Atribui papéis se fornecidos
            if (roles.Any())
            {
                var roleAssignResult = await AssignRolesToUserAsync(keycloakUserId, roles, adminToken.Value, cancellationToken);
                if (roleAssignResult.IsFailure)
                {
                    logger.LogWarning("User created but role assignment failed: {Error}", roleAssignResult.Error);
                    // Não falha na criação do usuário, apenas registra o aviso
                }
            }

            logger.LogInformation("User created successfully in Keycloak with ID: {UserId}", keycloakUserId);
            return Result<string>.Success(keycloakUserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while creating user in Keycloak. Payload: {Payload}",
                JsonSerializer.Serialize(new { username, email, firstName, lastName }, JsonOptions));
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

    public Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return Task.FromResult(Result<TokenValidationResult>.Failure("Invalid token format"));

            var jwt = tokenHandler.ReadJwtToken(token);

            // Verifica se o token expirou
            if (jwt.ValidTo < DateTime.UtcNow)
                return Task.FromResult(Result<TokenValidationResult>.Failure("Token has expired"));

            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var roles = jwt.Claims.Where(c => c.Type == "realm_access" || c.Type == "resource_access")
                .SelectMany(c => ExtractRolesFromClaim(c.Value))
                .Distinct()
                .ToList();

            var claims = jwt.Claims.ToDictionary(c => c.Type, c => (object)c.Value);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Task.FromResult(Result<TokenValidationResult>.Failure("Invalid user ID in token"));

            var validationResult = new TokenValidationResult(
                userGuid,
                roles,
                claims
            );

            return Task.FromResult(Result<TokenValidationResult>.Success(validationResult));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during token validation");
            return Task.FromResult(Result<TokenValidationResult>.Failure($"Token validation failed: {ex.Message}"));
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
            var json = JsonSerializer.Serialize(updatePayload, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

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
        // Verifica se temos um token válido
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
                JsonOptions, cancellationToken);

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
            logger.LogInformation("Assigning roles to user {UserId} with roles: {Roles}",
                keycloakUserId, string.Join(", ", roles));

            // 1. Obter papéis disponíveis do realm
            var availableRolesRequest = new HttpRequestMessage(HttpMethod.Get, 
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/roles");
            availableRolesRequest.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var availableRolesResponse = await httpClient.SendAsync(availableRolesRequest, cancellationToken);
            if (!availableRolesResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get available roles: {StatusCode}", availableRolesResponse.StatusCode);
                return Result.Failure("Failed to get available roles from Keycloak");
            }

            var availableRolesJson = await availableRolesResponse.Content.ReadAsStringAsync(cancellationToken);
            var availableRoles = JsonSerializer.Deserialize<KeycloakRole[]>(availableRolesJson, 
                JsonOptions) ?? [];

            // 2. Mapear nomes de papéis para objetos de papel
            var rolesToAssign = new List<KeycloakRole>();
            foreach (var roleName in roles)
            {
                var role = availableRoles.FirstOrDefault(r => 
                    string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
                
                if (role != null)
                {
                    rolesToAssign.Add(role);
                }
                else
                {
                    logger.LogWarning("Role '{RoleName}' not found in Keycloak realm", roleName);
                }
            }

            if (rolesToAssign.Count == 0)
            {
                logger.LogInformation("No valid roles to assign to user {UserId}", keycloakUserId);
                return Result.Success();
            }

            // 3. Atribuir papéis ao usuário
            var assignRolesRequest = new HttpRequestMessage(HttpMethod.Post,
                $"{_options.BaseUrl}/admin/realms/{_options.Realm}/users/{keycloakUserId}/role-mappings/realm");
            assignRolesRequest.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            var rolesJson = JsonSerializer.Serialize(rolesToAssign, JsonOptions);
            assignRolesRequest.Content = new StringContent(rolesJson, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            var assignRolesResponse = await httpClient.SendAsync(assignRolesRequest, cancellationToken);
            if (!assignRolesResponse.IsSuccessStatusCode)
            {
                var errorContent = await assignRolesResponse.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to assign roles to user {UserId}: {StatusCode} - {Error}", 
                    keycloakUserId, assignRolesResponse.StatusCode, errorContent);
                return Result.Failure($"Failed to assign roles: {assignRolesResponse.StatusCode}");
            }

            logger.LogInformation("Successfully assigned {RoleCount} roles to user {UserId}", 
                rolesToAssign.Count, keycloakUserId);
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
            // Analisa a estrutura JSON dos claims de papel do Keycloak
            // Os papéis podem vir em diferentes formatos:
            // 1. realm_access: { "roles": ["role1", "role2"] }
            // 2. resource_access: { "client1": { "roles": ["role1"] }, "client2": { "roles": ["role2"] } }
            
            var roles = new List<string>();
            
            using var document = JsonDocument.Parse(claimValue);
            var root = document.RootElement;
            
            // Verifica se é um claim realm_access
            if (root.TryGetProperty("roles", out var realmRoles) && realmRoles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in realmRoles.EnumerateArray())
                {
                    if (role.ValueKind == JsonValueKind.String)
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            roles.Add(roleValue);
                        }
                    }
                }
            }
            else
            {
                // Verifica se é um claim resource_access (papéis de cliente)
                foreach (var client in root.EnumerateObject())
                {
                    if (client.Value.TryGetProperty("roles", out var clientRoles) && 
                        clientRoles.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in clientRoles.EnumerateArray())
                        {
                            if (role.ValueKind == JsonValueKind.String)
                            {
                                var roleValue = role.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                {
                                    // Prefixo com nome do cliente para evitar conflitos
                                    roles.Add($"{client.Name}:{roleValue}");
                                }
                            }
                        }
                    }
                }
            }
            
            return roles.Distinct();
        }
        catch (JsonException)
        {
            // Se não conseguir analisar como JSON, pode ser um valor simples
            return string.IsNullOrEmpty(claimValue) ? Enumerable.Empty<string>() : new[] { claimValue };
        }
        catch
        {
            return [];
        }
    }
}