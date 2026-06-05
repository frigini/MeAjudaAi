using MeAjudaAi.Shared.Logging;
using Serilog;
using Serilog.Configuration;

namespace MeAjudaAi.Shared.Logging.Extensions;

/// <summary>
/// Extensões para configuração do CorrelationIdEnricher no Serilog
/// </summary>
public static class CorrelationIdEnricherExtensions
{
    /// <summary>
    /// Adiciona o enricher de Correlation ID à configuração do Serilog
    /// </summary>
    public static LoggerConfiguration WithCorrelationIdEnricher(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration == null)
            throw new ArgumentNullException(nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With<CorrelationIdEnricher>();
    }
}
