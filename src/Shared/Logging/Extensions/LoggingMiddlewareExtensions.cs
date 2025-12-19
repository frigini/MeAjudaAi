using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace MeAjudaAi.Shared.Logging.Extensions;

/// <summary>
/// Extension methods para adicionar contexto de logging
/// </summary>
public static class LoggingMiddlewareExtensions
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
    public static IDisposable PushUserContext(this ILogger logger, string? userId, string? username = null)
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
    public static IDisposable PushOperationContext(this ILogger logger, string operation, object? parameters = null)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("Operation", operation)
        };

        if (parameters != null)
            disposables.Add(LogContext.PushProperty("OperationParameters", parameters, destructureObjects: true));

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Helper para gerenciar múltiplos IDisposable
    /// </summary>
    private sealed class CompositeDisposable(List<IDisposable> disposables) : IDisposable
    {
        public void Dispose()
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }
}
