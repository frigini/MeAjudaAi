using System.Security.Claims;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.ApiService.Middleware;

/// <summary>
/// Middleware temporário para diagnóstico de problemas de autorização.
/// Adiciona headers com roles e permissões do usuário atual.
/// </summary>
public class DebugAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DebugAuthMiddleware> _logger;

    public DebugAuthMiddleware(RequestDelegate next, ILogger<DebugAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Executa o pipeline primeiro (para garantir que autenticação aconteceu)
        // Wait, AuthenticationMiddleware runs before this if we register it correctly.
        // We want to run logic AFTER Authentication.
        
        // Pass to next middleware first? No, we want to set headers BEFORE response starts.
        // But Identity is set BY AuthenticationMiddleware which must run BEFORE this.
        // So: Authentication -> DebugAuthMiddleware -> Authorization -> Endpoint
        
        await _next(context);
        
        // Headers só podem ser adicionados se a resposta não começou
        // Mas se a autorização falhar (403), o response já pode ter começado?
        // Authorization middleware writes 403.
        // So we need to use OnStarting.
        
        // BETTER: Logic inside Invoke, before Next, accessing User.
    }
}

// Simplified approach: Middleware that inspects context.User immediately.
// Assumption: registered after UseAuthentication.
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
