using Microsoft.AspNetCore.Builder;
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

        // Adicionar coleta periódica de métricas apenas em produção/staging
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
}