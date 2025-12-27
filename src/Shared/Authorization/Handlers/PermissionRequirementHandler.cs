using System.Security.Claims;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Authorization.Handlers;

/// <summary>
/// Authorization handler que verifica PermissionRequirement.
/// </summary>
public sealed class PermissionRequirementHandler(ILogger<PermissionRequirementHandler> logger) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;

        // Verifica se o usuário está autenticado
        if (user?.Identity?.IsAuthenticated != true)
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
        var requiredPermission = requirement.Permission.GetValue();
        var hasPermission = user.HasClaim(AuthConstants.Claims.Permission, requiredPermission);

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

    private static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? user.FindFirst("id")?.Value;
    }
}
