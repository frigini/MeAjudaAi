using System.Diagnostics;
using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para adicionar o middleware de métricas
/// </summary>
public static class BusinessMetricsMiddlewareExtensions
{
    /// <summary>
    /// Adiciona middleware de métricas de negócio
    /// </summary>
    public static IApplicationBuilder UseBusinessMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessMetricsMiddleware>();
    }
}

/// <summary>
/// Middleware para capturar métricas customizadas de negócio
/// </summary>
internal class BusinessMetricsMiddleware(
    RequestDelegate next,
    BusinessMetrics businessMetrics,
    ILogger<BusinessMetricsMiddleware> logger)
{
    private static readonly Regex IdPattern = new(@"/\d+", RegexOptions.Compiled);

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Capturar métricas de API
            var endpoint = GetEndpointName(context);
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            businessMetrics.RecordApiCall(endpoint, method, statusCode);

            // Log para endpoints específicos de negócio
            LogBusinessEvents(context, stopwatch.Elapsed);
        }
    }

    private void LogBusinessEvents(HttpContext context, TimeSpan elapsed)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        // Capturar eventos específicos de negócio
        if (path != null)
        {
            // Registros de usuário (aceita rotas versionadas como /api/v1/users)
            if ((path == "/api/users" || (path.StartsWith("/api/v") && path.EndsWith("/users"))) && 
                method == "POST" && statusCode is >= 200 and < 300)
            {
                businessMetrics.RecordUserRegistration("api");
                logger.LogInformation("User registration completed via API");
            }

            // Logins (aceita rotas versionadas como /api/v1/auth/login)
            if ((path == "/api/auth/login" || (path.StartsWith("/api/v") && path.EndsWith("/auth/login"))) && 
                method == "POST" && statusCode is >= 200 and < 300)
            {
                // User is unauthenticated at login endpoint, so we record as 'anonymous'
                // To track actual userId, implement post-authentication metric or extract from request body
                businessMetrics.RecordUserLogin("anonymous", "password");
                logger.LogInformation("User login completed");
            }

            // Solicitações de ajuda (aceita rotas versionadas como /api/v1/help-requests)
            if ((path == "/api/help-requests" || (path.StartsWith("/api/v") && path.Contains("/help-requests") && !path.Contains("/complete"))) && 
                method == "POST" && statusCode is >= 200 and < 300)
            {
                // Extrair categoria e urgência dos headers ou do corpo da requisição se necessário
                businessMetrics.RecordHelpRequestCreated("general", "normal");
                logger.LogInformation("Help request created");
            }

            // Conclusão de ajuda (aceita rotas versionadas como /api/v1/help-requests/{id}/complete)
            if (((path.StartsWith("/api/help-requests/") && path.EndsWith("/complete")) || 
                 (path.StartsWith("/api/v") && path.Contains("/help-requests/") && path.EndsWith("/complete"))) && 
                method == "POST" && statusCode is >= 200 and < 300)
            {
                businessMetrics.RecordHelpRequestCompleted("general", elapsed);
                logger.LogInformation("Help request completed in {ElapsedMs}ms", elapsed.TotalMilliseconds);
            }
        }
    }

    private static string GetEndpointName(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            return endpoint.DisplayName ?? context.Request.Path.Value ?? "unknown";
        }

        // Normalizar path para métricas (remover IDs específicos)
        var path = context.Request.Path.Value ?? "/";

        // Substituir IDs numéricos por placeholder
        var normalizedPath = IdPattern.Replace(path, "/{id}");

        return normalizedPath;
    }
}
