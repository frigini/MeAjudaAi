using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.ApiService.Handlers;

public class SelfOrAdminRequirement : IAuthorizationRequirement { }

public class SelfOrAdminHandler : AuthorizationHandler<SelfOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfOrAdminRequirement requirement)
    {
        // Se o usuário não está autenticado, falha imediatamente
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirst(MeAjudaAi.Shared.Utilities.Constants.AuthConstants.Claims.Subject)?.Value 
                          ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var roles = context.User.FindAll(MeAjudaAi.Shared.Utilities.Constants.AuthConstants.Claims.Roles)
            .Concat(context.User.FindAll(System.Security.Claims.ClaimTypes.Role))
            .Select(c => c.Value)
            .Distinct();

        // Verifica se o usuário é admin
        if (roles.Any(r =>
            string.Equals(r, MeAjudaAi.Shared.Utilities.UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, MeAjudaAi.Shared.Utilities.UserRoles.SuperAdmin, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Verifica se está acessando o próprio recurso
        if (context.Resource is HttpContext httpContext)
        {
            var routeUserId = httpContext.GetRouteValue("id")?.ToString()
                               ?? httpContext.GetRouteValue("userId")?.ToString();

            // Só permite acesso se ambos os IDs estão presentes e são iguais
            if (!string.IsNullOrWhiteSpace(userIdClaim) &&
                !string.IsNullOrWhiteSpace(routeUserId) &&
                string.Equals(userIdClaim, routeUserId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Se chegou até aqui, o usuário não tem permissão
        context.Fail();
        return Task.CompletedTask;
    }
}
