using System.Diagnostics;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace MeAjudaAi.Shared.Logging;

/// <summary>
/// Middleware para adicionar correlation ID e contexto enriquecido aos logs
/// </summary>
internal class LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Gerar ou usar correlation ID existente
        var correlationId = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault()
                           ?? UuidGenerator.NewIdString();

        // Adicionar correlation ID ao response header
        context.Response.Headers.TryAdd(AuthConstants.Headers.CorrelationId, correlationId);

        // Criar contexto de log enriquecido
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()))
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger.LogInformation("Request started {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await next(context);

                stopwatch.Stop();

                logger.LogInformation("Request completed {Method} {Path} - {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                logger.LogError(ex, "Request failed {Method} {Path} - {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                throw new InvalidOperationException(
                    $"Request failed: {context.Request.Method} {context.Request.Path} (Status: {context.Response.StatusCode}) after {stopwatch.ElapsedMilliseconds}ms",
                    ex);
            }
        }
    }
}
