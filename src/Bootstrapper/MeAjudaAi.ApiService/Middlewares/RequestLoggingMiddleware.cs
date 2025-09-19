using System.Diagnostics;

namespace MeAjudaAi.ApiService.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

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

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["ClientIp"] = clientIp,
            ["UserAgent"] = userAgent,
            ["UserId"] = userId
        });

        _logger.LogInformation(
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
            _logger.LogError(ex,
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

            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    elapsedMs
                );
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    elapsedMs
                );
            }
            else
            {
                _logger.LogInformation(
                    "Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    elapsedMs
                );
            }
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
}