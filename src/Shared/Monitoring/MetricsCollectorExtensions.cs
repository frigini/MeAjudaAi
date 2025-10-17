using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para registrar o serviço de coleta de métricas
/// </summary>
public static class MetricsCollectorExtensions
{
    /// <summary>
    /// Adiciona o serviço de coleta de métricas
    /// </summary>
    public static IServiceCollection AddMetricsCollector(this IServiceCollection services)
    {
        return services.AddHostedService<MetricsCollectorService>();
    }
}