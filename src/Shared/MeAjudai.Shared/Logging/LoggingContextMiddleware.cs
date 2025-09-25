using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Logging;

/// <summary>
/// Middleware para adicionar correlation ID e contexto enriquecido aos logs
/// </summary>
public class LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Gerar ou usar correlation ID existente
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? UuidGenerator.NewIdString();

        // Adicionar correlation ID ao response header
        context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);

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
                
                throw;
            }
        }
    }
}

/// <summary>
/// Extension methods para adicionar contexto de logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adiciona middleware de contexto de logging
    /// </summary>
    public static IApplicationBuilder UseLoggingContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LoggingContextMiddleware>();
    }

    /// <summary>
    /// Adiciona contexto de usuário aos logs
    /// </summary>
    public static IDisposable PushUserContext(this Microsoft.Extensions.Logging.ILogger logger, string? userId, string? username = null)
    {
        var disposables = new List<IDisposable>();
        
        if (!string.IsNullOrEmpty(userId))
            disposables.Add(LogContext.PushProperty("UserId", userId));
            
        if (!string.IsNullOrEmpty(username))
            disposables.Add(LogContext.PushProperty("Username", username));

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Adiciona contexto de operação aos logs
    /// </summary>
    public static IDisposable PushOperationContext(this Microsoft.Extensions.Logging.ILogger logger, string operation, object? parameters = null)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("Operation", operation)
        };

        if (parameters != null)
            disposables.Add(LogContext.PushProperty("OperationParameters", parameters, true));

        return new CompositeDisposable(disposables);
    }
}

/// <summary>
/// Classe helper para gerenciar múltiplos disposables
/// </summary>
internal class CompositeDisposable(List<IDisposable> disposables) : IDisposable
{
    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable?.Dispose();
        }
    }
}