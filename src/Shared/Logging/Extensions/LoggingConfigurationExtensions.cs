using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MeAjudaAi.Shared.Logging.Extensions;

/// <summary>
/// Extension methods para configuração de logging
/// </summary>
public static class LoggingConfigurationExtensions
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
}
