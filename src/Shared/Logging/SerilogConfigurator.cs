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
    public static void ConfigureSerilog(
        LoggerConfiguration loggerConfig,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // 📄 Ler configurações básicas do appsettings.json
        loggerConfig
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
            ConfigureApplicationInsights(configuration);
        }

        // Configurar correlation ID enricher
        config.Enrich.WithCorrelationIdEnricher();
    }

    /// <summary>
    /// Configura Application Insights para produção (futuro)
    /// </summary>
    private static void ConfigureApplicationInsights(IConfiguration configuration)
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
