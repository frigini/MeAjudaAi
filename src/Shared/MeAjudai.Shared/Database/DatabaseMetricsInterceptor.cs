using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace MeAjudaAi.Shared.Database;

public class DatabaseMetricsInterceptor : DbCommandInterceptor
{
    private readonly DatabaseMetrics _metrics;
    private readonly ILogger<DatabaseMetricsInterceptor> _logger;

    public DatabaseMetricsInterceptor(DatabaseMetrics metrics, ILogger<DatabaseMetricsInterceptor> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void RecordMetrics(DbCommand command, CommandExecutedEventData eventData)
    {
        var duration = eventData.Duration;
        var queryType = GetQueryType(command.CommandText);

        _metrics.RecordQuery(queryType, duration);

        if (duration.TotalMilliseconds > 1000) // Slow query threshold
        {
            _logger.LogWarning("Slow query: {Duration}ms - {QueryType}", duration.TotalMilliseconds, queryType);
        }
    }

    private static string GetQueryType(string commandText)
    {
        var trimmed = commandText.TrimStart().ToUpperInvariant();
        return trimmed switch
        {
            var text when text.StartsWith("SELECT") => "SELECT",
            var text when text.StartsWith("INSERT") => "INSERT", 
            var text when text.StartsWith("UPDATE") => "UPDATE",
            var text when text.StartsWith("DELETE") => "DELETE",
            _ => "OTHER"
        };
    }
}