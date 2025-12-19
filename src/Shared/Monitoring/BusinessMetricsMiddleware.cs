using System.Diagnostics;
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
            // Registros de usuário
            if (path.Contains("/users") && method == "POST" && statusCode is >= 200 and < 300)
            {
                businessMetrics.RecordUserRegistration("api");
                logger.LogInformation("User registration completed via API");
            }

            // Logins
            if (path.Contains("/auth/login") && method == "POST" && statusCode is >= 200 and < 300)
            {
                var userId = context.User?.FindFirst(AuthConstants.Claims.Subject)?.Value ?? "unknown";
                businessMetrics.RecordUserLogin(userId, "password");
                logger.LogInformation("User login completed: {UserId}", userId);
            }

            // Solicitações de ajuda
            if (path.Contains("/help-requests") && method == "POST" && statusCode is >= 200 and < 300)
            {
                // Extrair categoria e urgência dos headers ou do corpo da requisição se necessário
                businessMetrics.RecordHelpRequestCreated("general", "normal");
                logger.LogInformation("Help request created");
            }

            // Conclusão de ajuda
            if (path.Contains("/help-requests") && path.Contains("/complete") && method == "POST" && statusCode is >= 200 and < 300)
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
        var normalizedPath = System.Text.RegularExpressions.Regex.Replace(
            path, @"/\d+", "/{id}");

        return normalizedPath;
    }
}
