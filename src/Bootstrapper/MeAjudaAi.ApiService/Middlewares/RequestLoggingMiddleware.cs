using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace MeAjudaAi.ApiService.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private readonly Serilog.ILogger _logger = Log.ForContext<RequestLoggingMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging para health checks e static files
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        var clientIp = GetClientIpAddress(context);
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var userId = GetUserId(context);

        // Adiciona o RequestId no contexto para outros middlewares/endpoints
        context.Items["RequestId"] = requestId;

        var logger = _logger.ForContext("RequestId", requestId)
                            .ForContext("ClientIp", clientIp)
                            .ForContext("UserAgent", userAgent)
                            .ForContext("UserId", userId);

        logger.Information(
            "Starting request {Method} {Path} {QueryString} from {ClientIp} User: {UserId}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            clientIp,
            userId
        );

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Request {Method} {Path} failed with exception: {ExceptionMessage}",
                context.Request.Method,
                context.Request.Path,
                ex.Message
            );
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var level = GetLogLevel(context.Response.StatusCode);
            logger.Write(level,
                "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }

    private static bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        return path?.Contains("/health") == true ||
               path?.Contains("/metrics") == true ||
               path?.Contains("/swagger") == true ||
               path?.StartsWith("/css") == true ||
               path?.StartsWith("/js") == true ||
               path?.StartsWith("/images") == true;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Considera proxies e load balancers
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetUserId(HttpContext context)
    {
        return context.User?.FindFirst("sub")?.Value ??
               context.User?.FindFirst("id")?.Value ??
               "anonymous";
    }

    private static LogEventLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogEventLevel.Error,
            >= 400 => LogEventLevel.Warning,
            _ => LogEventLevel.Information
        };
    }
}