using Dapper;
using Npgsql;
using System.Diagnostics;

namespace MeAjudaAi.Shared.Database;

public class DapperConnection(PostgresOptions postgresOptions, DatabaseMetrics metrics) : IDapperConnection
{
    private readonly string _connectionString = postgresOptions?.ConnectionString
            ?? throw new InvalidOperationException("PostgreSQL connection string not found. Configure connection string via Aspire, 'Postgres:ConnectionString' in appsettings.json, or as ConnectionStrings:meajudaai-db");

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<T>(sql, param);
            
            stopwatch.Stop();
            metrics.RecordDapperQuery("query_multiple", stopwatch.Elapsed);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_query_multiple", ex);
            throw;
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QuerySingleOrDefaultAsync<T>(sql, param);
            
            stopwatch.Stop();
            metrics.RecordDapperQuery("query_single", stopwatch.Elapsed);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_query_single", ex);
            throw;
        }
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.ExecuteAsync(sql, param);
            
            stopwatch.Stop();
            metrics.RecordDapperQuery("execute", stopwatch.Elapsed);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metrics.RecordConnectionError("dapper_execute", ex);
            throw;
        }
    }
}