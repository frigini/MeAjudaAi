using System.Diagnostics;
using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Middleware para capturar métricas customizadas de negócio
/// </summary>
public class BusinessMetricsMiddleware(
    RequestDelegate next,
    BusinessMetrics businessMetrics,
    ILogger<BusinessMetricsMiddleware> logger)
{
    private static readonly Regex IdPattern = new(@"/\d+", RegexOptions.Compiled);
    private static readonly Regex VersionedApiPattern = new(@"^/api/v\d+(?:/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        var pathValue = context.Request.Path.Value;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        // Capturar eventos específicos de negócio
        if (!string.IsNullOrEmpty(pathValue))
        {
            var pathSpan = pathValue.AsSpan();

            // Registros de usuário (aceita rotas exatas como /api/users ou /api/v1/users)
            var isUserPath = pathSpan.Equals("/api/users", StringComparison.OrdinalIgnoreCase) || 
                             (pathSpan.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase) && 
                              pathSpan.Slice(5).IndexOf('/') > 0 && 
                              pathValue.EndsWith("/users", StringComparison.OrdinalIgnoreCase) &&
                              Regex.IsMatch(pathValue, @"^/api/v\d+/users$", RegexOptions.IgnoreCase));

            if (isUserPath && method == "POST" && statusCode is >= 200 and < 300)
            {
                businessMetrics.RecordUserRegistration("api");
                logger.LogInformation("User registration completed via API");
            }

            // Logins (aceita rotas exatas como /api/auth/login ou /api/v1/auth/login)
            var isLoginPath = pathSpan.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) || 
                              (pathSpan.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase) && 
                               Regex.IsMatch(pathValue, @"^/api/v\d+/auth/login$", RegexOptions.IgnoreCase));

            if (isLoginPath && method == "POST" && statusCode is >= 200 and < 300)
            {
                businessMetrics.RecordUserLogin("anonymous", "password");
                logger.LogInformation("User login completed");
            }

            // Solicitações de ajuda (aceita rotas versionadas exatas como /api/v1/help-requests)
            if ((pathSpan.Equals("/api/help-requests", StringComparison.OrdinalIgnoreCase) || 
                 Regex.IsMatch(pathValue, @"^/api/v\d+/help-requests$", RegexOptions.IgnoreCase)) && 
                method == "POST" && statusCode is >= 200 and < 300)
            {
                // Extrair categoria e urgência dos headers ou do corpo da requisição se necessário
                businessMetrics.RecordHelpRequestCreated("general", "normal");
                logger.LogInformation("Help request created");
            }

            // Conclusão de ajuda (aceita rotas versionadas como /api/v1/help-requests/{id}/complete)
            if (((pathSpan.StartsWith("/api/help-requests/", StringComparison.OrdinalIgnoreCase) && pathSpan.EndsWith("/complete", StringComparison.OrdinalIgnoreCase)) || 
                 (VersionedApiPattern.IsMatch(pathValue) && pathSpan.Contains("/help-requests/", StringComparison.OrdinalIgnoreCase) && pathSpan.EndsWith("/complete", StringComparison.OrdinalIgnoreCase))) && 
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
