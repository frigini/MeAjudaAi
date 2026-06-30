using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Keycloak.Models;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MeAjudaAi.Shared.Authorization.Keycloak;

/// <summary>
/// Implementação do resolver de permissões que integra com Keycloak
/// para obter roles e mapear para permissões do sistema.
/// </summary>
public sealed class KeycloakPermissionResolver : IKeycloakPermissionResolver
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;
    private readonly ICacheService _cache;
    private readonly ILogger<KeycloakPermissionResolver> _logger;

    public string ModuleName => "*"; // Keycloak resolve permissões para todos os módulos

    public KeycloakPermissionResolver(
        HttpClient httpClient,
        IConfiguration configuration,
        ICacheService cache,
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
    /// Mascara um ID de usuário para fins de log, a fim de evitar a exposição de PII (Informações Pessoalmente Identificáveis).
    /// </summary>
    private static string MaskUserId(string userId) => PiiMaskingHelper.MaskUserId(userId);

    /// <summary>
    /// Opções de cache estáticas para o armazenamento de roles.
    /// </summary>
    private static readonly HybridCacheEntryOptions RoleCacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(15),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<IReadOnlyList<EPermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<EPermission>();

        try
        {
            var cacheKey = $"keycloak_user_roles_{HashForCacheKey(userId)}";
            var userRoles = await _cache.GetOrCreateAsync(
                cacheKey,
                async ValueTask<IReadOnlyList<string>> (ct) => await GetUserRolesFromKeycloakAsync(userId, ct),
                options: RoleCacheOptions,
                cancellationToken: cancellationToken);

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
            var statusCode = ex is HttpRequestException hre ? hre.StatusCode?.ToString() : null;
            _logger.LogError("Failed to resolve permissions from Keycloak for user {MaskedUserId} ({ExceptionType}, Status: {StatusCode})",
                MaskUserId(userId), ex.GetType().Name, statusCode ?? "N/A");
            return Array.Empty<EPermission>();
        }
    }

    public bool CanResolve(EPermission permission) => true;

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
            _logger.LogWarning("User {MaskedUserId} not found in Keycloak (HTTP {StatusCode})", MaskUserId(userId), ex.StatusCode);
            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving roles from Keycloak for user {MaskedUserId}: {ExceptionType}", MaskUserId(userId), ex.GetType().Name);
            throw new InvalidOperationException(
                $"Failed to retrieve user roles from Keycloak for user ID: {MaskUserId(userId)}",
                ex);
        }
    }

    /// <summary>
    /// Obtém token de admin do Keycloak.
    /// </summary>
    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var cacheKey = "keycloak_admin_token";

        var (cachedToken, isCached) = await _cache.GetAsync<TokenResponse>(cacheKey, cancellationToken);
        if (isCached && cachedToken != null)
        {
            return cachedToken.AccessToken;
        }

        var tokenResponse = await RequestAdminTokenAsync(cancellationToken);
        
        var margin = TimeSpan.FromSeconds(60);
        var expiration = tokenResponse.ExpiresIn > 60 
            ? TimeSpan.FromSeconds(tokenResponse.ExpiresIn) - margin 
            : TimeSpan.FromSeconds(Math.Max(30, tokenResponse.ExpiresIn / 2));
        
        await _cache.SetAsync(cacheKey, tokenResponse, expiration, cancellationToken: cancellationToken);

        return tokenResponse.AccessToken;
    }

    private async Task<TokenResponse> RequestAdminTokenAsync(CancellationToken cancellationToken)
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

        return tokenData ?? throw new InvalidOperationException("Failed to get admin token");
    }

    /// <summary>
    /// Busca informações do usuário no Keycloak.
    /// Primeiro tenta por ID, depois por username como fallback.
    /// </summary>
    private async Task<KeycloakUser?> GetUserInfoAsync(string userId, string adminToken, CancellationToken cancellationToken)
    {
        var endpoint = $"{_config.BaseUrl}/admin/realms/{_config.Realm}/users";

        // Primeiro, tenta buscar diretamente por ID do Keycloak (mais eficiente)
        try
        {
            var encodedUserId = Uri.EscapeDataString(userId);
            using var directRequest = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/{encodedUserId}");
            directRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            using var directResponse = await _httpClient.SendAsync(directRequest, cancellationToken);
            if (directResponse.IsSuccessStatusCode)
            {
                var userJson = await directResponse.Content.ReadAsStringAsync(cancellationToken);
                var user = JsonSerializer.Deserialize<KeycloakUser>(userJson);
                if (user != null)
                {
                    _logger.LogDebug("User {MaskedUserId} found by ID in Keycloak", MaskUserId(userId));
                    return user;
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("User {MaskedUserId} not found by ID (HTTP {StatusCode}), trying username search", MaskUserId(userId), ex.StatusCode);
        }

        // Fallback: busca por username
        try
        {
            var encodedUserId = Uri.EscapeDataString(userId);
            using var searchRequest = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}?username={encodedUserId}&exact=true");
            searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            using var searchResponse = await _httpClient.SendAsync(searchRequest, cancellationToken);
            searchResponse.EnsureSuccessStatusCode();

            var usersJson = await searchResponse.Content.ReadAsStringAsync(cancellationToken);
            var users = JsonSerializer.Deserialize<KeycloakUser[]>(usersJson);

            var foundUser = users?.FirstOrDefault();
            if (foundUser != null)
            {
                _logger.LogDebug("User {MaskedUserId} found by username in Keycloak", MaskUserId(userId));
            }

            return foundUser;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                "Failed to find user {MaskedUserId} by username in Keycloak (HTTP error: {ExceptionType})",
                MaskUserId(userId),
                ex.GetType().Name);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                "Failed to find user {MaskedUserId} by username in Keycloak (JSON error: {ExceptionType})",
                MaskUserId(userId),
                ex.GetType().Name);
            return null;
        }
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
    public IEnumerable<EPermission> MapKeycloakRoleToPermissions(string keycloakRole)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakRole);

        var normalizedRole = keycloakRole.ToLowerInvariant();

        return normalizedRole switch
        {
            // Roles de sistema
            var r when r == RoleConstants.Admin.ToLowerInvariant() ||
                       r == RoleConstants.SystemAdmin.ToLowerInvariant() => new[]
            {
                EPermission.AdminSystem,
                EPermission.AdminUsers,
                EPermission.AdminReports,
                EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersDelete, EPermission.UsersList, EPermission.UsersProfile,
                EPermission.ProvidersRead, EPermission.ProvidersCreate, EPermission.ProvidersUpdate, EPermission.ProvidersDelete, EPermission.ProvidersList, EPermission.ProvidersApprove,
                EPermission.BookingsRead, EPermission.BookingsCreate, EPermission.BookingsUpdate, EPermission.BookingsCancel, EPermission.BookingsList, EPermission.BookingsManage,
                EPermission.ReportsView, EPermission.ReportsExport, EPermission.ReportsCreate, EPermission.ReportsAdmin,
                EPermission.ServiceCatalogsRead, EPermission.ServiceCatalogsManage,
                EPermission.LocationsRead, EPermission.LocationsManage,
                EPermission.PaymentsRead, EPermission.PaymentsManage, EPermission.PaymentsCheckout,
                EPermission.CommunicationsRead, EPermission.CommunicationsSend, EPermission.CommunicationsManage,
                EPermission.RatingsRead, EPermission.RatingsCreate, EPermission.RatingsModerate,
                EPermission.SearchRead, EPermission.SearchManage,
                EPermission.DocumentsRead, EPermission.DocumentsUpload, EPermission.DocumentsVerify, EPermission.DocumentsDelete
            },

            // Roles de administração de usuários
            var r when r == RoleConstants.UserAdmin.ToLowerInvariant() => new[]
            {
                EPermission.AdminUsers,
                EPermission.UsersRead, EPermission.UsersCreate, EPermission.UsersUpdate, EPermission.UsersList, EPermission.UsersProfile
            },

            // Roles de operação de usuários
            var r when r == RoleConstants.UserOperator.ToLowerInvariant() => new[]
            {
                EPermission.UsersRead, EPermission.UsersUpdate, EPermission.UsersList, EPermission.UsersProfile
            },

            // Usuário básico
            var r when r == RoleConstants.User.ToLowerInvariant() => new[]
            {
                EPermission.UsersRead, EPermission.UsersProfile, EPermission.BookingsRead, EPermission.BookingsCreate, EPermission.RatingsCreate
            },

            // Roles de prestadores
            var r when r == RoleConstants.ProviderAdmin.ToLowerInvariant() => new[]
            {
                EPermission.ProvidersRead, EPermission.ProvidersCreate, EPermission.ProvidersUpdate, EPermission.ProvidersDelete, EPermission.ProvidersList,
                EPermission.BookingsRead, EPermission.BookingsUpdate, EPermission.BookingsList
            },

            var r when r == RoleConstants.Provider.ToLowerInvariant() => new[]
            {
                EPermission.ProvidersRead, EPermission.ProvidersUpdate, EPermission.BookingsRead, EPermission.BookingsUpdate, EPermission.BookingsList
            },

            // Roles de agendamentos
            var r when r == "booking_admin" || r == RoleConstants.OrderAdmin.ToLowerInvariant() => new[]
            {
                EPermission.BookingsRead, EPermission.BookingsCreate, EPermission.BookingsUpdate, EPermission.BookingsCancel, EPermission.BookingsList, EPermission.BookingsManage
            },

            var r when r == "booking_operator" || r == RoleConstants.OrderOperator.ToLowerInvariant() => new[]
            {
                EPermission.BookingsRead, EPermission.BookingsUpdate, EPermission.BookingsList
            },

            // Roles de relatórios
            var r when r == RoleConstants.ReportAdmin.ToLowerInvariant() => new[]
            {
                EPermission.ReportsView, EPermission.ReportsExport, EPermission.ReportsCreate
            },

            var r when r == RoleConstants.ReportViewer.ToLowerInvariant() => new[]
            {
                EPermission.ReportsView
            },

            // Roles de gerenciamento de catálogo de serviços
            var r when r == RoleConstants.CatalogManager.ToLowerInvariant() => new[]
            {
                EPermission.ServiceCatalogsRead,
                EPermission.ServiceCatalogsManage
            },

            // Roles de gerenciamento de localidades
            var r when r == RoleConstants.LocationManager.ToLowerInvariant() => new[]
            {
                EPermission.LocationsRead,
                EPermission.LocationsManage
            },

            // Role desconhecida
            _ => Array.Empty<EPermission>()
        };
    }

    /// <summary>
    /// Gera o hash de uma string para uso em chaves de cache, prevenindo a exposição de PII.
    /// </summary>
    private static string HashForCacheKey(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
