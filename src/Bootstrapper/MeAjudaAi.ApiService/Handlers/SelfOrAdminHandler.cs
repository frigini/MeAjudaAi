using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.ApiService.Handlers;

public class SelfOrAdminRequirement : IAuthorizationRequirement { }

public class SelfOrAdminHandler : AuthorizationHandler<SelfOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfOrAdminRequirement requirement)
    {
        // 1. Se o usuário não está autenticado, falha imediatamente
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // 2. Extrai todas as roles de forma robusta (suporta 'roles' e ClaimTypes.Role)
        var userRoles = context.User.FindAll(AuthConstants.Claims.Roles)
            .Concat(context.User.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Distinct();

        // 3. Verifica se o usuário é admin
        if (userRoles.Any(role => RoleConstants.AdminEquivalentRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 4. Verifica se está acessando o próprio recurso (validação de ID)
        if (context.Resource is HttpContext httpContext)
        {
            var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value 
                              ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var routeUserId = httpContext.GetRouteValue("userId")?.ToString()
                               ?? httpContext.GetRouteValue("id")?.ToString()
                               ?? httpContext.GetRouteValue("providerId")?.ToString();

            if (!string.IsNullOrWhiteSpace(userIdClaim) &&
                !string.IsNullOrWhiteSpace(routeUserId) &&
                string.Equals(userIdClaim, routeUserId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // 5. Se nenhuma condição foi atendida, falha explicitamente
        context.Fail();
        return Task.CompletedTask;
    }
}
