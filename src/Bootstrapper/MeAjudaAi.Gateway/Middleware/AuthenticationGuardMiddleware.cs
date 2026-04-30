using System.Text.Json;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Gateway.Middleware;

/// <summary>
/// Middleware de guarda de autenticação no edge (Gateway).
/// Implementa defense-in-depth verificando autenticação antes de encaminhar
/// requisições para rotas protegidas via YARP.
///
/// Rotas públicas (configuradas em <see cref="PublicRoutesOptions"/>) são
/// encaminhadas sem verificação de token. Todas as outras rotas exigem um
/// token JWT válido já validado pelo <c>UseAuthentication()</c> do ASP.NET Core.
/// </summary>
public class AuthenticationGuardMiddleware(
    RequestDelegate next,
    IOptionsMonitor<PublicRoutesOptions> options,
    ILogger<AuthenticationGuardMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var publicRoutes = options.CurrentValue.Routes;

        var isPublicRoute = publicRoutes.Any(route =>
            path.StartsWith(route, StringComparison.OrdinalIgnoreCase));

        if (isPublicRoute)
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            logger.LogWarning(
                "Acesso não autenticado bloqueado no Gateway. Path: {Path}, IP: {IP}",
                path,
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Unauthorized",
                message = "Autenticação necessária para acessar este recurso."
            };

            var json = JsonSerializer.Serialize(errorResponse, SerializationDefaults.Api);
            await context.Response.WriteAsync(json);
            return;
        }

        await next(context);
    }
}