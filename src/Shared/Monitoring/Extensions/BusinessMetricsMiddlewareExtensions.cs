using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Extension methods para adicionar o middleware de métricas
/// </summary>
public static class BusinessMetricsMiddlewareExtensions
{
    /// <summary>
    /// Adiciona middleware de métricas de negócio
    /// </summary>
    public static IApplicationBuilder UseBusinessMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessMetricsMiddleware>();
    }
}
