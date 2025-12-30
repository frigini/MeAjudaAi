using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Serilog.Events;

namespace MeAjudaAi.Shared.Logging;

/// <summary>
/// Extension methods consolidados para configuração de Logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adiciona configuração de Serilog
    /// </summary>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Usar services.AddSerilog() que registra DiagnosticContext automaticamente
        services.AddSerilog((serviceProvider, loggerConfig) =>
        {
            // Aplicar a configuração do SerilogConfigurator (modifica loggerConfig diretamente)
            SerilogConfigurator.ConfigureSerilog(loggerConfig, configuration, environment);

            // Sinks configurados aqui (Console + File)
            loggerConfig.WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}");

            // File sink para persistência
            loggerConfig.WriteTo.File("logs/app-.log",
                rollingInterval: Serilog.RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        });

        return services;
    }

    /// <summary>
    /// Adiciona middleware de contexto de logging
    /// </summary>
    public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder app)
    {
        app.UseLoggingContext();

        // Only use Serilog request logging if not in Testing environment
        var environment = app.ApplicationServices.GetService<IWebHostEnvironment>();
        if (environment != null && !environment.IsEnvironment("Testing"))
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null)
                        return LogEventLevel.Error;
                    if (httpContext.Response.StatusCode > 499)
                        return LogEventLevel.Error;
                    return LogEventLevel.Information;
                };

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown");

                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        var userId = httpContext.User.FindFirst("sub")?.Value;
                        var username = httpContext.User.FindFirst("preferred_username")?.Value;

                        if (!string.IsNullOrEmpty(userId))
                            diagnosticContext.Set("UserId", userId);
                        if (!string.IsNullOrEmpty(username))
                            diagnosticContext.Set("Username", username);
                    }
                };
            });
        }

        return app;
    }

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
