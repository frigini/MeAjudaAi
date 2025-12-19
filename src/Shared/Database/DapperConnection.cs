using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            HandleDapperError(ex, "query_multiple", sql);
            throw; // Inalcançável mas necessário para o compilador
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
            HandleDapperError(ex, "query_single", sql);
            throw; // Inalcançável mas necessário para o compilador
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
            HandleDapperError(ex, "execute", sql);
            throw; // Inalcançável mas necessário para o compilador
        }
    }

    [DoesNotReturn]
    private void HandleDapperError(Exception ex, string operationType, string? sql)
    {
        metrics.RecordConnectionError($"dapper_{operationType}", ex);
        // Registra preview do SQL apenas quando Debug está habilitado para reduzir exposição em prod + evitar custo de formatação
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var sqlPreview = GetSqlPreview(sql);
            logger.LogDebug("Operação Dapper falhou (tipo: {OperationType}). Preview do SQL: {SqlPreview}",
                operationType, sqlPreview);
        }
        logger.LogError(ex, "Falha ao executar operação Dapper (tipo: {OperationType})", operationType);
        throw new InvalidOperationException($"Falha ao executar operação Dapper (tipo: {operationType})", ex);
    }

    private static string? GetSqlPreview(string? sql)
    {
        if (sql is null)
            return null;
        
        return sql.Length > 100 ? sql[..100] + "..." : sql;
    }
}
