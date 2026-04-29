using System.Diagnostics;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.ApiService.Middlewares;

/// <summary>
/// Middleware para logging estruturado de requisições HTTP.
/// Registra início/fim de cada request com métricas de performance, IP do cliente e contexto do usuário.
/// </summary>
/// <remarks>
/// <para><b>Propósito</b>: Rastreabilidade completa de requisições para auditoria e debugging</para>
/// <para><b>Informações Registradas</b>:</para>
/// <list type="bullet">
///   <item>RequestId único (correlação entre logs)</item>
///   <item>IP do cliente e User-Agent</item>
///   <item>UserId (se autenticado)</item>
///   <item>Tempo de execução (ElapsedMs)</item>
///   <item>Status code da resposta</item>
/// </list>
/// <para><b>Uso</b>: Registrado automaticamente no pipeline via <see cref="MiddlewareExtensions.UseApiMiddlewares"/></para>
/// <para><b>Observação</b>: Health checks e arquivos estáticos são ignorados para reduzir ruído nos logs</para>
/// </remarks>
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
        var requestId = UuidGenerator.NewIdString();
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
        var pathValue = context.Request.Path.Value;
        if (string.IsNullOrEmpty(pathValue)) return false;
        
        var pathSpan = pathValue.AsSpan();
        
        return pathSpan.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
               pathSpan.Contains("/metrics", StringComparison.OrdinalIgnoreCase) ||
               pathSpan.Contains("/swagger", StringComparison.OrdinalIgnoreCase) ||
               pathSpan.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
               pathSpan.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
               pathSpan.StartsWith("/images", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Considera proxies e load balancers
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            var span = xForwardedFor.AsSpan();
            var commaIndex = span.IndexOf(',');
            if (commaIndex >= 0) span = span[..commaIndex];
            return span.Trim().ToString();
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
