using Serilog;
using Serilog.Configuration;

namespace MeAjudaAi.Shared.Logging.Extensions;

/// <summary>
/// Extension methods para registrar o enricher de Correlation ID
/// </summary>
public static class CorrelationIdEnricherExtensions
{
    /// <summary>
    /// Adiciona o enricher de correlation ID
    /// </summary>
    public static LoggerConfiguration WithCorrelationIdEnricher(this LoggerEnrichmentConfiguration enrichConfiguration)
    {
        return enrichConfiguration.With<CorrelationIdEnricher>();
    }
}
