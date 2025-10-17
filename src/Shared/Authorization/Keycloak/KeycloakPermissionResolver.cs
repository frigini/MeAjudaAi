using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization.Keycloak;

/// <summary>
/// Implementação do resolver de permissões que integra com Keycloak
/// para obter roles e mapear para permissões do sistema.
/// </summary>
public sealed class KeycloakPermissionResolver : IKeycloakPermissionResolver
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;
    private readonly HybridCache _cache;
    private readonly ILogger<KeycloakPermissionResolver> _logger;

    public string ModuleName => ModuleNames.Users; // Keycloak resolver é usado principalmente pelo módulo Users

    public KeycloakPermissionResolver(
        HttpClient httpClient,
        IConfiguration configuration,
        HybridCache cache,
        ILogger<KeycloakPermissionResolver> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        _httpClient = httpClient;
        _config = configuration.GetSection("Keycloak").Get<KeycloakConfiguration>()
                  ?? throw new InvalidOperationException("Keycloak configuration not found");
        
        // Validate required configuration values
        if (string.IsNullOrWhiteSpace(_config.BaseUrl))
            throw new InvalidOperationException("Keycloak BaseUrl is required but not configured");
        if (string.IsNullOrWhiteSpace(_config.Realm))
            throw new InvalidOperationException("Keycloak Realm is required but not configured");
        if (string.IsNullOrWhiteSpace(_config.AdminClientId))
            throw new InvalidOperationException("Keycloak AdminClientId is required but not configured");
        if (string.IsNullOrWhiteSpace(_config.AdminClientSecret))
            throw new InvalidOperationException("Keycloak AdminClientSecret is required but not configured");
        
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Masks a user ID for logging purposes to avoid exposing PII.
    /// </summary>
    private static string MaskUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "[EMPTY]";
        
        if (userId.Length <= 6)
            return $"{userId[0]}***{userId[^1]}";
        
        return $"{userId[..3]}***{userId[^3..]}";
    }

    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        // Converte UserId para string para compatibilidade com a implementação atual
        return await ResolvePermissionsAsync(userId.Value.ToString(), cancellationToken);
    }

    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<EPermission>();

        try
        {
            // Cache key para roles do usuário
            var cacheKey = $"keycloak_user_roles_{userId}";
            var cacheOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(15), // Cache roles por 15 minutos
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };

            // Busca roles do cache ou Keycloak
            var userRoles = await _cache.GetOrCreateAsync(
                cacheKey,
                async _ => await GetUserRolesFromKeycloakAsync(userId, cancellationToken),
                cacheOptions,
                cancellationToken: cancellationToken);

            // Mapeia roles para permissões
            var permissions = new HashSet<EPermission>();
            foreach (var role in userRoles)
            {
                var rolePermissions = MapKeycloakRoleToPermissions(role);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }

            _logger.LogDebug("Resolved {PermissionCount} permissions from {RoleCount} Keycloak roles for user {MaskedUserId}",
                permissions.Count, userRoles.Count, MaskUserId(userId));

            return permissions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve permissions from Keycloak for user {MaskedUserId}", MaskUserId(userId));
            return Array.Empty<EPermission>();
        }
    }

    public bool CanResolve(EPermission permission)
    {
        // Este resolver pode processar qualquer permissão pois consulta diretamente o Keycloak
        return true;
    }

    /// <summary>
    /// Busca roles do usuário no Keycloak via Admin API.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetUserRolesFromKeycloakAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Obter token de admin
            var adminToken = await GetAdminTokenAsync(cancellationToken);
            
            // 2. Buscar usuário pelo ID
            var userInfo = await GetUserInfoAsync(userId, adminToken, cancellationToken);
            if (userInfo == null)
            {
                _logger.LogWarning("User {MaskedUserId} not found in Keycloak", MaskUserId(userId));
                return Array.Empty<string>();
            }

            // 3. Buscar roles do usuário
            var userRoles = await GetUserRolesAsync(userInfo.Id, adminToken, cancellationToken);
            
            _logger.LogDebug("Retrieved {RoleCount} roles from Keycloak for user {MaskedUserId}", userRoles.Count, MaskUserId(userId));
            
            return userRoles;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User {MaskedUserId} not found in Keycloak", MaskUserId(userId));
            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles from Keycloak for user {MaskedUserId}", MaskUserId(userId));
            throw;
        }
    }

    /// <summary>
    /// Obtém token de admin do Keycloak.
    /// </summary>
    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "keycloak_admin_token";
        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5), // Tokens duram mais, mas cache por menos tempo para segurança
            LocalCacheExpiration = TimeSpan.FromMinutes(2)
        };

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await RequestAdminTokenAsync(cancellationToken),
            cacheOptions,
            cancellationToken: cancellationToken);
    }

    private async Task<string> RequestAdminTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"{_config.BaseUrl}/realms/{_config.Realm}/protocol/openid-connect/token";
        
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _config.AdminClientId },
            { "client_secret", _config.AdminClientSecret }
        };

        using var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponse);
        
        return tokenData?.AccessToken ?? throw new InvalidOperationException("Failed to get admin token");
    }

    /// <summary>
    /// Busca informações do usuário no Keycloak.
    /// </summary>
    private async Task<KeycloakUser?> GetUserInfoAsync(string userId, string adminToken, CancellationToken cancellationToken)
    {
        var endpoint = $"{_config.BaseUrl}/admin/realms/{_config.Realm}/users";
        var encodedUserId = Uri.EscapeDataString(userId);
        
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}?username={encodedUserId}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var usersJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var users = JsonSerializer.Deserialize<KeycloakUser[]>(usersJson);
        
        return users?.FirstOrDefault();
    }

    /// <summary>
    /// Busca roles do usuário no Keycloak.
    /// </summary>
    private async Task<IReadOnlyList<string>> GetUserRolesAsync(string keycloakUserId, string adminToken, CancellationToken cancellationToken)
    {
        var endpoint = $"{_config.BaseUrl}/admin/realms/{_config.Realm}/users/{keycloakUserId}/role-mappings/realm";
        
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var rolesJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var roles = JsonSerializer.Deserialize<KeycloakRole[]>(rolesJson);
        
        return roles?.Select(r => r.Name).ToList() ?? new List<string>();
    }

    /// <summary>
    /// Mapeia roles do Keycloak para permissões do sistema.
    /// </summary>
    public IEnumerable<EPermission> MapKeycloakRoleToPermissions(string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);
        
        return roleName.ToLowerInvariant() switch
        {
            // Roles de sistema
            "meajudaai-system-admin" => new[]
            {
                EPermission.AdminSystem,
                EPermission.AdminUsers,
                EPermission.AdminReports,
                EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersDelete, EPermission.UsersList,
                EPermission.ProvidersRead, EPermission.ProvidersCreate, EPermission.ProvidersUpdate, EPermission.ProvidersDelete,
                EPermission.OrdersRead, EPermission.OrdersCreate, EPermission.OrdersUpdate, EPermission.OrdersDelete,
                EPermission.ReportsView, EPermission.ReportsExport, EPermission.ReportsCreate
            },
            
            // Roles de administração de usuários
            "meajudaai-user-admin" => new[]
            {
                EPermission.AdminUsers,
                EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersList
            },
            
            // Roles de operação de usuários
            "meajudaai-user-operator" => new[]
            {
                EPermission.UsersRead, EPermission.UsersUpdate, EPermission.UsersList
            },
            
            // Usuário básico
            "meajudaai-user" => new[]
            {
                EPermission.UsersRead, EPermission.UsersProfile
            },
            
            // Roles de prestadores
            "meajudaai-provider-admin" => new[]
            {
                EPermission.ProvidersRead, EPermission.ProvidersCreate, EPermission.ProvidersUpdate, EPermission.ProvidersDelete
            },
            
            "meajudaai-provider" => new[]
            {
                EPermission.ProvidersRead
            },
            
            // Roles de pedidos
            "meajudaai-order-admin" => new[]
            {
                EPermission.OrdersRead, EPermission.OrdersCreate, EPermission.OrdersUpdate, EPermission.OrdersDelete
            },
            
            "meajudaai-order-operator" => new[]
            {
                EPermission.OrdersRead, EPermission.OrdersUpdate
            },
            
            // Roles de relatórios
            "meajudaai-report-admin" => new[]
            {
                EPermission.ReportsView, EPermission.ReportsExport, EPermission.ReportsCreate
            },
            
            "meajudaai-report-viewer" => new[]
            {
                EPermission.ReportsView
            },
            
            // Role desconhecida
            _ => Array.Empty<EPermission>()
        };
    }
}

/// <summary>
/// Configuração para integração com Keycloak.
/// </summary>
public sealed class KeycloakConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string AdminClientId { get; set; } = string.Empty;
    public string AdminClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Resposta do token do Keycloak.
/// </summary>
internal sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

/// <summary>
/// Representação do usuário no Keycloak.
/// </summary>
internal sealed class KeycloakUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

/// <summary>
/// Representação do role no Keycloak.
/// </summary>
internal sealed class KeycloakRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
