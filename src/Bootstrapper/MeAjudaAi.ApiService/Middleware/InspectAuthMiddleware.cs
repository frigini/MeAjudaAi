using System.Security.Claims;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.ApiService.Middleware;


/// <summary>
/// Middleware para diagnóstico de problemas de autorização.
/// Adiciona headers com roles e permissões do usuário atual.
/// Registrado após UseAuthentication para ter acesso ao User.
/// </summary>
/// <remarks>
/// Headers de debug são emitidos apenas em ambientes não-produção.
/// </remarks>
public class InspectAuthMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Headers de debug devem ser emitidos apenas em ambientes não-produção
        if (!env.IsProduction())
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var permissions = context.User.FindAll(AuthConstants.Claims.Permission).Select(c => c.Value).ToList();
                var name = context.User.Identity.Name ?? "unknown";

                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[AuthConstants.Headers.DebugUser] = name;
                    context.Response.Headers[AuthConstants.Headers.DebugRoles] = string.Join(", ", roles);
                    context.Response.Headers[AuthConstants.Headers.DebugPermissionsCount] = permissions.Count.ToString();
                    
                    // Trunca permissões para evitar estouro do tamanho do header se houver muitas
                    var permString = string.Join(", ", permissions);
                    if (permString.Length > 1000)
                    {
                        permString = string.Concat(permString.AsSpan(0, 1000), "...");
                    }
                    
                    context.Response.Headers[AuthConstants.Headers.DebugPermissions] = permString;
                    return Task.CompletedTask;
                });
            }
            else
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[AuthConstants.Headers.DebugAuthStatus] = "Not Authenticated";
                    return Task.CompletedTask;
                });
            }
        }

        await next(context);
    }
}
