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

        var userIdClaim = context.User.FindFirst("sub")?.Value;
        var roles = context.User.FindAll("roles").Select(c => c.Value);

        // Verifica se o usuário é admin
        if (roles.Any(r =>
            string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "super-admin", StringComparison.OrdinalIgnoreCase)))
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
                string.Equals(userIdClaim, routeUserId, StringComparison.Ordinal))
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
