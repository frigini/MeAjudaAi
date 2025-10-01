using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Métricas essenciais para monitoramento de database.
/// Foca apenas no necessário para detectar problemas de performance.
/// </summary>
public sealed class DatabaseMetrics
{
    private readonly Counter<long> _queryCount;
    private readonly Counter<long> _slowQueryCount;
    private readonly Histogram<double> _queryDuration;
    private readonly Counter<long> _connectionErrors;

    public DatabaseMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MeAjudaAi.Database");

        _queryCount = meter.CreateCounter<long>(
            "database_queries_total",
            description: "Total number of database queries executed");

        _slowQueryCount = meter.CreateCounter<long>(
            "database_slow_queries_total",
            description: "Total number of slow database queries (>1s)");

        _queryDuration = meter.CreateHistogram<double>(
            "database_query_duration_seconds",
            unit: "s",
            description: "Duration of database queries in seconds");

        _connectionErrors = meter.CreateCounter<long>(
            "database_connection_errors_total",
            description: "Total number of database connection errors");
    }

    /// <summary>
    /// Registra a execução de uma query
    /// </summary>
    public void RecordQuery(string operation, TimeSpan duration, bool isSuccess = true)
    {
        var durationSeconds = duration.TotalSeconds;

        // Contabiliza query
        _queryCount.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("success", isSuccess));

        // Registra duração
        _queryDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("success", isSuccess));

        // Query lenta (>1s)
        if (durationSeconds > 1.0)
        {
            _slowQueryCount.Add(1,
                new KeyValuePair<string, object?>("operation", operation));
        }
    }

    /// <summary>
    /// Registra erro de conexão
    /// </summary>
    public void RecordConnectionError(string operation, Exception exception)
    {
        _connectionErrors.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("error_type", exception.GetType().Name));
    }

    /// <summary>
    /// Helper para registrar query com contexto automático
    /// </summary>
    public void RecordEntityFrameworkQuery(string entityType, string operation, TimeSpan duration)
    {
        RecordQuery($"ef_{entityType}_{operation}", duration);
    }

    /// <summary>
    /// Helper para registrar query Dapper
    /// </summary>
    public void RecordDapperQuery(string queryName, TimeSpan duration)
    {
        RecordQuery($"dapper_{queryName}", duration);
    }
}