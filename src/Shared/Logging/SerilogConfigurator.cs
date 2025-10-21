using MeAjudaAi.Shared.Authorization.Keycloak;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MeAjudaAi.Shared.Logging;

/// <summary>
/// Configurador híbrido do Serilog - combina appsettings.json com lógica C#
/// </summary>
public static class SerilogConfigurator
{
    /// <summary>
    /// Configura o Serilog usando abordagem híbrida:
    /// - Configurações básicas do appsettings.json
    /// - Enrichers e lógica específica por ambiente via código
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var loggerConfig = new LoggerConfiguration()
            // 📄 Ler configurações básicas do appsettings.json
            .ReadFrom.Configuration(configuration)

            // 🏗️ Adicionar enrichers via código
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MeAjudaAi")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithProperty("Version", GetApplicationVersion())

            // 🔒 Redact sensitive data from KeycloakPermissionOptions
            .Destructure.ByTransforming<KeycloakPermissionOptions>(o => new
            {
                o.BaseUrl,
                o.Realm,
                o.ClientId,
                o.AdminUsername,
                ClientSecret = "***REDACTED***",
                AdminPassword = "***REDACTED***"
            });

        // 🎯 Aplicar configurações específicas por ambiente
        ApplyEnvironmentSpecificConfiguration(loggerConfig, configuration, environment);

        return loggerConfig;
    }

    /// <summary>
    /// Aplica configurações específicas por ambiente que não são facilmente
    /// expressas em JSON
    /// </summary>
    private static void ApplyEnvironmentSpecificConfiguration(
        LoggerConfiguration config,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Development: Logs mais verbosos e formatação amigável
            config
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information);
        }
        else if (environment.IsProduction())
        {
            // Production: Logs estruturados e otimizados
            config
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);

            // Configurar Application Insights se disponível
            ConfigureApplicationInsights(config, configuration);
        }

        // Configurar correlation ID enricher
        config.Enrich.WithCorrelationIdEnricher();
    }

    /// <summary>
    /// Configura Application Insights para produção (futuro)
    /// </summary>
    private static void ConfigureApplicationInsights(LoggerConfiguration config, IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Application Insights será implementado no futuro quando necessário
        }
    }

    public static string GetApplicationVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "unknown";
    }
}

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
            // Aplicar a configuração do SerilogConfigurator
            var configuredLogger = SerilogConfigurator.ConfigureSerilog(configuration, environment);

            loggerConfig.ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "MeAjudaAi")
                .Enrich.WithProperty("Environment", environment.EnvironmentName)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                .Enrich.WithProperty("Version", SerilogConfigurator.GetApplicationVersion());

            // Aplicar configurações específicas do ambiente
            if (environment.IsDevelopment())
            {
                loggerConfig
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Information);
            }
            else
            {
                loggerConfig
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning);
            }

            // Console sink
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
                options.GetLevel = (httpContext, elapsed, ex) => ex != null
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 499
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;

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
