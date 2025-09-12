using Microsoft.AspNetCore.Authorization;

namespace MeAjudaAi.ApiService.Handlers;

public class SelfOrAdminRequirement : IAuthorizationRequirement { }

public class SelfOrAdminHandler : AuthorizationHandler<SelfOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfOrAdminRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("sub")?.Value;
        var roles = context.User.FindAll("role").Select(c => c.Value);

        // Check if user is admin
        if (roles.Any(r => r == "Admin" || r == "SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if accessing own resource
        if (context.Resource is HttpContext httpContext)
        {
            var routeUserId = httpContext.GetRouteValue("id")?.ToString();
            if (userIdClaim == routeUserId)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}