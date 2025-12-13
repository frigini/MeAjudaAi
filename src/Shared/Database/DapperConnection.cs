using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Shared.Database;

public class DapperConnection(PostgresOptions postgresOptions, DatabaseMetrics metrics, ILogger<DapperConnection> logger) : IDapperConnection
{
    private readonly string _connectionString = GetConnectionString(postgresOptions);

    private static string GetConnectionString(PostgresOptions? postgresOptions)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Testing")
        {
            // Em ambiente de teste, usa uma connection string mock se não houver uma configurada
            return postgresOptions?.ConnectionString ?? "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=test;";
        }

        return postgresOptions?.ConnectionString
            ?? throw new InvalidOperationException("PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db");
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var connection = new NpgsqlConnection(_connectionString);
            var commandDefinition = new CommandDefinition(sql, param, cancellationToken: cancellationToken);
            var result = await connection.QueryAsync<T>(commandDefinition);

            stopwatch.Stop();
            metrics.RecordDapperQuery("query_multiple", stopwatch.Elapsed);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            // Não registrar cancelamentos como erros - são esperados em casos normais
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_query_multiple", ex);
            // Log SQL preview only in debug/development contexts to avoid exposing schema in production
            logger.LogDebug("Dapper query failed (type: multiple). SQL preview: {SqlPreview}",
                sql?.Length > 100 ? sql.Substring(0, 100) + "..." : sql);
            logger.LogError(ex, "Failed to execute Dapper query (type: multiple)");
            throw new InvalidOperationException("Failed to execute Dapper query (type: multiple)", ex);
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var connection = new NpgsqlConnection(_connectionString);
            var commandDefinition = new CommandDefinition(sql, param, cancellationToken: cancellationToken);
            var result = await connection.QuerySingleOrDefaultAsync<T>(commandDefinition);

            stopwatch.Stop();
            metrics.RecordDapperQuery("query_single", stopwatch.Elapsed);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            // Não registrar cancelamentos como erros - são esperados em casos normais
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_query_single", ex);
            // Log SQL preview only in debug/development contexts to avoid exposing schema in production
            logger.LogDebug("Dapper query failed (type: single). SQL preview: {SqlPreview}",
                sql?.Length > 100 ? sql.Substring(0, 100) + "..." : sql);
            logger.LogError(ex, "Failed to execute Dapper query (type: single)");
            throw new InvalidOperationException("Failed to execute Dapper query (type: single)", ex);
        }
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var connection = new NpgsqlConnection(_connectionString);
            var commandDefinition = new CommandDefinition(sql, param, cancellationToken: cancellationToken);
            var result = await connection.ExecuteAsync(commandDefinition);

            stopwatch.Stop();
            metrics.RecordDapperQuery("execute", stopwatch.Elapsed);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            // Não registrar cancelamentos como erros - são esperados em casos normais
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_execute", ex);
            // Log SQL preview only in debug/development contexts to avoid exposing schema in production
            logger.LogDebug("Dapper command failed (type: execute). SQL preview: {SqlPreview}",
                sql?.Length > 100 ? sql.Substring(0, 100) + "..." : sql);
            logger.LogError(ex, "Failed to execute Dapper command (type: execute)");
            throw new InvalidOperationException("Failed to execute Dapper command (type: execute)", ex);
        }
    }
}
