using System.Security.Claims;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.ApiService.Middleware;


/// <summary>
/// Middleware para diagnóstico de problemas de autorização.
/// Adiciona headers com roles e permissões do usuário atual.
/// Registrado após UseAuthentication para ter acesso ao User.
/// </summary>
public class InspectAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = context.User.FindAll(AuthConstants.Claims.Permission).Select(c => c.Value).ToList();
            var name = context.User.Identity.Name ?? "unknown";

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Debug-User"] = name;
                context.Response.Headers["X-Debug-Roles"] = string.Join(", ", roles);
                context.Response.Headers["X-Debug-Permissions-Count"] = permissions.Count.ToString();
                
                // Truncate permissions to avoid header size overflow if too many
                var permString = string.Join(", ", permissions);
                if (permString.Length > 1000) permString = permString.Substring(0, 1000) + "...";
                
                context.Response.Headers["X-Debug-Permissions"] = permString;
                return Task.CompletedTask;
            });
        }
        else
        {
             context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Debug-Auth-Status"] = "Not Authenticated";
                return Task.CompletedTask;
            });
        }

        await next(context);
    }
}
