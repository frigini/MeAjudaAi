using MeAjudaAi.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Shared.Authorization;

/// <summary>
/// Authorization handler que verifica se o usuário possui a permissão necessária.
/// </summary>
public sealed class PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<RequirePermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequirePermissionAttribute requirement)
    {
        var user = context.User;
        
        // Verifica se o usuário está autenticado
        if (!user.Identity?.IsAuthenticated == true)
        {
            logger.LogDebug("User is not authenticated");
            context.Fail();
            return Task.CompletedTask;
        }
        
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Could not extract user ID from claims");
            context.Fail();
            return Task.CompletedTask;
        }
        
        // Verifica se o usuário possui a permissão específica
        var requiredPermission = requirement.PermissionValue;
        var hasPermission = user.HasClaim(CustomClaimTypes.Permission, requiredPermission);
        
        if (hasPermission)
        {
            logger.LogDebug("User {UserId} has required permission {Permission}", 
                userId, requiredPermission);
            context.Succeed(requirement);
        }
        else
        {
            logger.LogDebug("User {UserId} lacks required permission {Permission}", 
                userId, requiredPermission);
            context.Fail();
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Extrai o ID do usuário dos claims.
    /// </summary>
    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user.FindFirst(AuthConstants.Claims.Subject)?.Value ??
               user.FindFirst("id")?.Value;
    }
}