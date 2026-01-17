using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.AppHost.Extensions;

/// <summary>
/// Serviço para configuração automática do Keycloak (clients OIDC, roles, usuários).
/// Executa automaticamente no startup em ambiente de desenvolvimento.
/// </summary>
public sealed class KeycloakSetupService(ILogger<KeycloakSetupService> logger, IHostEnvironment environment)
{
    private const string DefaultKeycloakUrl = "http://localhost:8080";
    private const string DefaultRealmName = "meajudaai";
    private const string DefaultAdminUsername = "admin";
    
    public async Task<bool> EnsureKeycloakConfiguredAsync(CancellationToken cancellationToken = default)
    {
        // Apenas em desenvolvimento
        if (!environment.IsDevelopment())
        {
            logger.LogInformation("Keycloak setup skipped (not in development environment)");
            return true;
        }

        logger.LogInformation("Checking Keycloak configuration...");

        try
        {
            // Aguardar Keycloak estar pronto
            if (!await WaitForKeycloakAsync(cancellationToken))
            {
                logger.LogWarning("Keycloak not available, skipping setup");
                return false;
            }

            // Obter token admin
            var adminPassword = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";
            var token = await GetAdminTokenAsync(DefaultAdminUsername, adminPassword, cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                logger.LogError("Failed to obtain Keycloak admin token");
                return false;
            }

            using var client = new HttpClient { BaseAddress = new Uri(DefaultKeycloakUrl) };
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);

            // Verificar/criar realm
            if (!await EnsureRealmExistsAsync(client, cancellationToken))
            {
                return false;
            }

            // Criar clients OIDC
            await EnsureClientExistsAsync(client, CreateAdminPortalClient(), cancellationToken);
            await EnsureClientExistsAsync(client, CreateCustomerAppClient(), cancellationToken);

            // Criar roles
            await EnsureRoleExistsAsync(client, "admin", cancellationToken);
            await EnsureRoleExistsAsync(client, "customer", cancellationToken);
            await EnsureRoleExistsAsync(client, "operator", cancellationToken);
            await EnsureRoleExistsAsync(client, "viewer", cancellationToken);

            // Criar usuários demo
            await EnsureUserExistsAsync(client, "admin.portal", "admin123", "admin", cancellationToken);
            await EnsureUserExistsAsync(client, "customer.demo", "customer123", "customer", cancellationToken);

            logger.LogInformation("Keycloak configuration completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to configure Keycloak");
            return false;
        }
    }

    private async Task<bool> WaitForKeycloakAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var maxAttempts = 30; // 30 seconds
        
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await client.GetAsync($"{DefaultKeycloakUrl}/health/ready", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Keycloak is ready");
                    return true;
                }
            }
            catch
            {
                // Keycloak ainda não está pronto
            }

            await Task.Delay(1000, cancellationToken);
        }

        return false;
    }

    private async Task<string?> GetAdminTokenAsync(string username, string password, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = username,
            ["password"] = password
        });

        try
        {
            var response = await client.PostAsync(
                $"{DefaultKeycloakUrl}/realms/master/protocol/openid-connect/token",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get admin token: {Status}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            return result?.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting admin token");
            return null;
        }
    }

    private async Task<bool> EnsureRealmExistsAsync(HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync($"/admin/realms/{DefaultRealmName}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Realm '{Realm}' already exists", DefaultRealmName);
                return true;
            }

            // Criar realm
            var realm = new
            {
                realm = DefaultRealmName,
                enabled = true,
                displayName = "MeAjudaAi",
                registrationAllowed = false,
                loginWithEmailAllowed = true,
                duplicateEmailsAllowed = false,
                resetPasswordAllowed = true,
                rememberMe = true,
                verifyEmail = false,
                loginTheme = "keycloak",
                accountTheme = "keycloak",
                emailTheme = "keycloak",
                sslRequired = "none" // Development only
            };

            var createResponse = await client.PostAsJsonAsync("/admin/realms", realm, cancellationToken);
            if (createResponse.IsSuccessStatusCode)
            {
                logger.LogInformation("Realm '{Realm}' created successfully", DefaultRealmName);
                return true;
            }

            logger.LogError("Failed to create realm: {Status}", createResponse.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring realm exists");
            return false;
        }
    }

    private async Task<bool> EnsureClientExistsAsync(HttpClient client, object clientConfig, CancellationToken cancellationToken)
    {
        try
        {
            var clientJson = JsonSerializer.Serialize(clientConfig);
            var clientObj = JsonSerializer.Deserialize<JsonElement>(clientJson);
            var clientId = clientObj.GetProperty("clientId").GetString()!;

            // Verificar se já existe
            var response = await client.GetAsync(
                $"/admin/realms/{DefaultRealmName}/clients?clientId={clientId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var existingClients = await response.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken);
                if (existingClients?.Length > 0)
                {
                    logger.LogInformation("Client '{ClientId}' already exists", clientId);
                    return true;
                }
            }

            // Criar client
            var createResponse = await client.PostAsJsonAsync(
                $"/admin/realms/{DefaultRealmName}/clients",
                clientConfig,
                cancellationToken);

            if (createResponse.IsSuccessStatusCode || createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                logger.LogInformation("Client '{ClientId}' configured successfully", clientId);
                return true;
            }

            logger.LogError("Failed to create client '{ClientId}': {Status}", clientId, createResponse.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring client exists");
            return false;
        }
    }

    private async Task<bool> EnsureRoleExistsAsync(HttpClient client, string roleName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync(
                $"/admin/realms/{DefaultRealmName}/roles/{roleName}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogDebug("Role '{Role}' already exists", roleName);
                return true;
            }

            var role = new { name = roleName, description = $"Role {roleName}" };
            var createResponse = await client.PostAsJsonAsync(
                $"/admin/realms/{DefaultRealmName}/roles",
                role,
                cancellationToken);

            if (createResponse.IsSuccessStatusCode || createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                logger.LogInformation("Role '{Role}' created successfully", roleName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring role exists");
            return false;
        }
    }

    private async Task<bool> EnsureUserExistsAsync(
        HttpClient client,
        string username,
        string password,
        string roleName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verificar se usuário existe
            var response = await client.GetAsync(
                $"/admin/realms/{DefaultRealmName}/users?username={username}&exact=true",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken);
                if (users?.Length > 0)
                {
                    logger.LogDebug("User '{Username}' already exists", username);
                    return true;
                }
            }

            // Criar usuário
            var user = new
            {
                username,
                enabled = true,
                emailVerified = true,
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

            var createResponse = await client.PostAsJsonAsync(
                $"/admin/realms/{DefaultRealmName}/users",
                user,
                cancellationToken);

            if (createResponse.IsSuccessStatusCode || createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                logger.LogInformation("User '{Username}' created successfully", username);
                
                // Atribuir role (best effort)
                await AssignRoleToUserAsync(client, username, roleName, cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring user exists");
            return false;
        }
    }

    private async Task AssignRoleToUserAsync(
        HttpClient client,
        string username,
        string roleName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Obter ID do usuário
            var userResponse = await client.GetAsync(
                $"/admin/realms/{DefaultRealmName}/users?username={username}&exact=true",
                cancellationToken);

            if (!userResponse.IsSuccessStatusCode) return;

            var users = await userResponse.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken);
            if (users?.Length == 0) return;

            var userId = users[0].GetProperty("id").GetString();

            // Obter role
            var roleResponse = await client.GetAsync(
                $"/admin/realms/{DefaultRealmName}/roles/{roleName}",
                cancellationToken);

            if (!roleResponse.IsSuccessStatusCode) return;

            var role = await roleResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

            // Atribuir role
            var roles = new[] { role };
            await client.PostAsJsonAsync(
                $"/admin/realms/{DefaultRealmName}/users/{userId}/role-mappings/realm",
                roles,
                cancellationToken);

            logger.LogInformation("Role '{Role}' assigned to user '{Username}'", roleName, username);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to assign role to user (non-critical)");
        }
    }

    private static object CreateAdminPortalClient() => new
    {
        clientId = "admin-portal",
        name = "Admin Portal - Blazor WebAssembly",
        description = "MeAjudaAi Admin Portal OIDC Client",
        enabled = true,
        publicClient = true,
        protocol = "openid-connect",
        standardFlowEnabled = true,
        implicitFlowEnabled = false,
        directAccessGrantsEnabled = false,
        serviceAccountsEnabled = false,
        redirectUris = new[]
        {
            "https://localhost:7032/*",
            "https://localhost:7281/*",
            "https://admin.meajudaai.com.br/*"
        },
        webOrigins = new[]
        {
            "https://localhost:7032",
            "https://localhost:7281",
            "https://admin.meajudaai.com.br"
        },
        attributes = new Dictionary<string, string>
        {
            ["pkce.code.challenge.method"] = "S256"
        }
    };

    private static object CreateCustomerAppClient() => new
    {
        clientId = "customer-app",
        name = "Customer App - Web + Mobile",
        description = "MeAjudaAi Customer App OIDC Client",
        enabled = true,
        publicClient = true,
        protocol = "openid-connect",
        standardFlowEnabled = true,
        implicitFlowEnabled = false,
        directAccessGrantsEnabled = false,
        serviceAccountsEnabled = false,
        redirectUris = new[]
        {
            "https://localhost:7282/*",
            "https://app.meajudaai.com.br/*",
            "meajudaai://callback"
        },
        webOrigins = new[]
        {
            "https://localhost:7282",
            "https://app.meajudaai.com.br"
        },
        attributes = new Dictionary<string, string>
        {
            ["pkce.code.challenge.method"] = "S256"
        }
    };

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken);
}
