using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para registrar as métricas customizadas
/// </summary>
public static class BusinessMetricsExtensions
{
    /// <summary>
    /// Adiciona métricas de negócio ao DI container
    /// </summary>
    public static IServiceCollection AddBusinessMetrics(this IServiceCollection services)
    {
        return services.AddSingleton<BusinessMetrics>();
    }
}
