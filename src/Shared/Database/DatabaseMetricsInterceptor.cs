using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Database;

public class DatabaseMetricsInterceptor(DatabaseMetrics metrics, ILogger<DatabaseMetricsInterceptor> logger) : DbCommandInterceptor
{
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData.Duration);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        RecordMetrics(command, eventData.Duration);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var queryType = GetQueryType(command.CommandText);
        metrics.RecordQuery(queryType, eventData.Duration, isSuccess: false);
        
        logger.LogError(eventData.Exception, "Database command failed: {QueryType}", queryType);
        
        await base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    internal void RecordMetrics(DbCommand command, TimeSpan duration, bool isSuccess = true)
    {
        var queryType = GetQueryType(command.CommandText);

        metrics.RecordQuery(queryType, duration, isSuccess);

        if (isSuccess && duration.TotalMilliseconds > 1000) // Limite de consulta lenta
        {
            logger.LogWarning("Slow query: {Duration}ms - {QueryType}", duration.TotalMilliseconds, queryType);
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
