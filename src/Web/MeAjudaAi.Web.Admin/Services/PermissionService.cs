using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using MeAjudaAi.Web.Admin.Authorization;
using System.Security.Claims;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Implementação do serviço de verificação de permissões.
/// </summary>
public class PermissionService(
    AuthenticationStateProvider authenticationStateProvider,
    IAuthorizationService authorizationService,
    ILogger<PermissionService> logger) : IPermissionService
{
    // Constantes para claims do Keycloak
    private const string RolesClaim = "roles";

    public async Task<bool> HasPermissionAsync(string policyName)
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                logger.LogDebug("User is not authenticated");
                return false;
            }

            var result = await authorizationService.AuthorizeAsync(user, policyName);
            
            logger.LogDebug(
                "Permission check for policy {PolicyName}: {Result}", 
                policyName, 
                result.Succeeded);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking permission for policy {PolicyName}", policyName);
            return false;
        }
    }

    public async Task<bool> HasAnyRoleAsync(params string[] roles)
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            var userRoles = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Verifica a claim personalizada 'roles' do Keycloak
            var keycloakRoles = user.FindAll(RolesClaim)
                .Select(c => c.Value)
                .ToList();

            userRoles.AddRange(keycloakRoles);

            var hasRole = roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

            logger.LogDebug(
                "Role check - Required: [{RequiredRoles}], User has: [{UserRoles}], Result: {Result}",
                string.Join(", ", roles),
                string.Join(", ", userRoles),
                hasRole);

            return hasRole;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking roles {Roles}", string.Join(", ", roles));
            return false;
        }
    }

    public async Task<bool> HasAllRolesAsync(params string[] roles)
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return false;
            }

            var userRoles = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(user.FindAll(RolesClaim).Select(c => c.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var hasAllRoles = roles.All(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

            logger.LogDebug(
                "All roles check - Required: [{RequiredRoles}], User has: [{UserRoles}], Result: {Result}",
                string.Join(", ", roles),
                string.Join(", ", userRoles),
                hasAllRoles);

            return hasAllRoles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking all roles {Roles}", string.Join(", ", roles));
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync()
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return Enumerable.Empty<string>();
            }

            var roles = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Concat(user.FindAll(RolesClaim).Select(c => c.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            logger.LogDebug("User roles: [{Roles}]", string.Join(", ", roles));

            return roles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user roles");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsAdminAsync()
    {
        return await HasAnyRoleAsync(RoleNames.Admin);
    }
}
