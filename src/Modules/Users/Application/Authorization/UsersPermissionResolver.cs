using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Authorization;

/// <summary>
/// Resolver de permissões específico para o módulo Users.
/// Integra com Keycloak para obter roles do usuário e mapear para permissões do sistema.
/// </summary>
public sealed class UsersPermissionResolver : IModulePermissionResolver
{
    private readonly ILogger<UsersPermissionResolver> _logger;
    private readonly IKeycloakPermissionResolver? _keycloakResolver;
    private readonly bool _useKeycloak;

    public string ModuleName => "Users";

    public UsersPermissionResolver(
        ILogger<UsersPermissionResolver> logger, 
        IConfiguration configuration,
        IKeycloakPermissionResolver? keycloakResolver = null)
    {
        _logger = logger;
        _keycloakResolver = keycloakResolver;
        
        // Environment variable para controlar se usa Keycloak ou mock
        _useKeycloak = configuration.GetValue("Authorization:UseKeycloak", false);
        
        if (_useKeycloak && _keycloakResolver == null)
        {
            _logger.LogWarning("Keycloak integration is enabled but IKeycloakPermissionResolver is not available. Falling back to mock implementation.");
            _useKeycloak = false;
        }
        
        _logger.LogInformation("UsersPermissionResolver initialized with {ResolverType} implementation", 
            _useKeycloak ? "Keycloak" : "Mock");
    }

    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        
        try
        {
            var permissions = new List<EPermission>();
            
            // Usa Keycloak ou implementação mock baseado na configuração
            var userRoles = _useKeycloak 
                ? await GetUserRolesFromKeycloakAsync(userId, cancellationToken).ConfigureAwait(false)
                : await GetUserRolesMockAsync(userId, cancellationToken).ConfigureAwait(false);
            
            foreach (var role in userRoles)
            {
                var rolePermissions = MapRoleToUserPermissions(role);
                permissions.AddRange(rolePermissions);
            }
            
            // Remove duplicatas
            var distinctPermissions = permissions.Distinct().ToList();
            
            _logger.LogDebug("Resolved {PermissionCount} Users module permissions for user {UserId} from roles: {Roles} using {ResolverType}", 
                distinctPermissions.Count, userId, string.Join(", ", userRoles), _useKeycloak ? "Keycloak" : "Mock");
            
            return distinctPermissions;
        }
        catch (OperationCanceledException)
        {
            // Operação cancelada pelo usuário ou timeout
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve Users module permissions for user {UserId}", userId);
            return [];
        }
    }
    
    public bool CanResolve(EPermission permission)
    {
        // Verifica se a permissão pertence ao módulo Users
        return permission.GetModule().Equals("Users", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Obtém roles do usuário via Keycloak.
    /// </summary>
    private async Task<IReadOnlyList<string>> GetUserRolesFromKeycloakAsync(string userId, CancellationToken cancellationToken)
    {
        if (_keycloakResolver == null)
        {
            _logger.LogWarning("Keycloak resolver not available, falling back to mock implementation");
            return await GetUserRolesMockAsync(userId, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            _logger.LogDebug("Fetching user roles from Keycloak for user {UserId}", userId);
            
            // Usa o Keycloak resolver para obter permissões e depois extrai os roles
            var permissions = await _keycloakResolver.ResolvePermissionsAsync(userId, cancellationToken).ConfigureAwait(false);
            
            // Converte permissões de volta para roles para manter compatibilidade
            // Em implementação real, o Keycloak resolver poderia expor um método para obter roles diretamente
            var roles = new List<string>();
            
            if (permissions.Contains(EPermission.AdminUsers))
                roles.Add("meajudaai-system-admin");
            else if (permissions.Contains(EPermission.UsersList))
                roles.Add("meajudaai-user-admin");
            else if (permissions.Contains(EPermission.UsersRead))
                roles.Add("meajudaai-user");
                
            _logger.LogDebug("Retrieved {RoleCount} roles from Keycloak for user {UserId}: {Roles}", 
                roles.Count, userId, string.Join(", ", roles));
                
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch roles from Keycloak for user {UserId}, falling back to mock", userId);
            return await GetUserRolesMockAsync(userId, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Obtém roles do usuário usando implementação mock/local.
    /// </summary>
    private async Task<IReadOnlyList<string>> GetUserRolesMockAsync(string userId, CancellationToken cancellationToken)
    {
        // Simula delay de consulta à base de dados ou serviço externo
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);

        // Mapeamento baseado em padrões de userId para desenvolvimento/testes
        var roles = userId switch
        {
            var id when id.Contains("admin", StringComparison.OrdinalIgnoreCase) => new[] { "meajudaai-system-admin", "meajudaai-user-admin" },
            var id when id.Contains("manager", StringComparison.OrdinalIgnoreCase) => new[] { "meajudaai-user-admin" },
            _ => new[] { "meajudaai-user" }
        };

        _logger.LogDebug("Retrieved {RoleCount} mock roles for user {UserId}: {Roles}", 
            roles.Length, userId, string.Join(", ", roles));
            
        return roles;
    }

    /// <summary>
    /// Mapeia roles para permissões específicas do módulo Users.
    /// </summary>
    private static EPermission[] MapRoleToUserPermissions(string role)
    {
        return role.ToUpperInvariant() switch
        {
            "MEAJUDAAI-SYSTEM-ADMIN" => [EPermission.UsersRead, EPermission.UsersUpdate, EPermission.UsersDelete, EPermission.AdminUsers],
            "MEAJUDAAI-USER-ADMIN" => [EPermission.UsersRead, EPermission.UsersUpdate, EPermission.UsersList],
            "MEAJUDAAI-USER" => [EPermission.UsersRead, EPermission.UsersProfile],
            _ => []
        };
    }
}