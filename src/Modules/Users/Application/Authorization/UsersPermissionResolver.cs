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

    public string ModuleName => ModuleNames.Users;

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

    public async Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);
        
        try
        {
            IReadOnlyList<EPermission> allPermissions;
            
            // Usa Keycloak ou implementação mock baseado na configuração
            // Converte o UserId para string para compatibilidade com implementações existentes
            allPermissions = _useKeycloak 
                ? await GetUserPermissionsFromKeycloakAsync(userId.Value.ToString(), cancellationToken).ConfigureAwait(false)
                : await GetUserPermissionsFromMockAsync(userId.Value.ToString(), cancellationToken).ConfigureAwait(false);
            
            // Filtra apenas permissões do módulo Users
            var usersPermissions = allPermissions
                .Where(permission => CanResolve(permission))
                .Distinct()
                .ToList();
            
            _logger.LogDebug("Resolved {PermissionCount} Users module permissions for user {UserId} using {ResolverType}", 
                usersPermissions.Count, userId, _useKeycloak ? "Keycloak" : "Mock");
            
            return usersPermissions;
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
        return permission.GetModule().Equals(ModuleNames.Users, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Obtém as permissões do usuário diretamente do Keycloak.
    /// Utiliza o resolver Keycloak existente para obter permissões sem conversão desnecessária.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões resolvidas pelo Keycloak</returns>
    private async Task<IReadOnlyList<EPermission>> GetUserPermissionsFromKeycloakAsync(string userId, CancellationToken cancellationToken)
    {
        if (_keycloakResolver == null)
        {
            _logger.LogWarning("Keycloak resolver is not available. Returning empty permissions for user {UserId}", userId);
            return [];
        }

        try
        {
            _logger.LogDebug("Fetching user permissions from Keycloak for user {UserId}", userId);
            
            var permissions = await _keycloakResolver.ResolvePermissionsAsync(userId, cancellationToken).ConfigureAwait(false);
            
            _logger.LogDebug("Retrieved {PermissionCount} permissions from Keycloak for user {UserId}", 
                permissions.Count, userId);
            
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permissions from Keycloak for user {UserId}", userId);
            return [];
        }
    }

    /// <summary>
    /// Obtém permissões do usuário usando implementação mock/local.
    /// Retorna permissões diretamente baseadas no padrão do userId para desenvolvimento/testes.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de permissões simuladas</returns>
    private async Task<IReadOnlyList<EPermission>> GetUserPermissionsFromMockAsync(string userId, CancellationToken cancellationToken)
    {
        // Simula delay de consulta à base de dados ou serviço externo
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);

        // Mapeamento baseado em padrões de userId para desenvolvimento/testes
        // Inclui apenas permissões do módulo Users (users:*)
        var permissions = userId switch
        {
            var id when id.Contains("admin", StringComparison.OrdinalIgnoreCase) => 
                new[] { 
                    EPermission.UsersRead, 
                    EPermission.UsersCreate, 
                    EPermission.UsersUpdate, 
                    EPermission.UsersDelete, 
                    EPermission.UsersList,
                    EPermission.UsersProfile
                },
            var id when id.Contains("manager", StringComparison.OrdinalIgnoreCase) => 
                new[] { 
                    EPermission.UsersRead, 
                    EPermission.UsersCreate, 
                    EPermission.UsersUpdate, 
                    EPermission.UsersList,
                    EPermission.UsersProfile 
                },
            _ => new[] { EPermission.UsersRead, EPermission.UsersProfile }
        };

        _logger.LogDebug("Retrieved {PermissionCount} mock permissions for user {UserId}", 
            permissions.Length, userId);
            
        return permissions;
    }
}