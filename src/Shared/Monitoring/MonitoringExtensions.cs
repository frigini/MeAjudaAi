using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para configurar monitoramento avançado
/// </summary>
public static class MonitoringExtensions
{
    /// <summary>
    /// Adiciona monitoramento avançado complementar ao Aspire
    /// </summary>
    public static IServiceCollection AddAdvancedMonitoring(this IServiceCollection services, IHostEnvironment environment)
    {
        // Adicionar métricas customizadas de negócio
        services.AddBusinessMetrics();

        // Adicionar health checks customizados
        services.AddMeAjudaAiHealthChecks();

        // Adicionar coleta periódica de métricas apenas em produção
        if (!environment.IsDevelopment())
        {
            services.AddMetricsCollector();
        }

        return services;
    }

    /// <summary>
    /// Configura middleware de monitoramento
    /// </summary>
    public static IApplicationBuilder UseAdvancedMonitoring(this IApplicationBuilder app)
    {
        // Adicionar middleware de métricas de negócio
        app.UseBusinessMetrics();

        return app;
    }

    /// <summary>
    /// Adiciona métricas de negócio ao DI container
    /// </summary>
    public static IServiceCollection AddBusinessMetrics(this IServiceCollection services)
    {
        return services.AddSingleton<BusinessMetrics>();
    }

    /// <summary>
    /// Adiciona middleware de métricas de negócio
    /// </summary>
    public static IApplicationBuilder UseBusinessMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessMetricsMiddleware>();
    }
}
