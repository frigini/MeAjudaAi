using Dapper;
using Npgsql;

namespace MeAjudaAi.Shared.Database;

public class DapperConnection(PostgresOptions postgresOptions) : IDapperConnection
{
    private readonly string _connectionString = postgresOptions?.ConnectionString
            ?? throw new InvalidOperationException("PostgreSQL connection string not found. Configure 'Postgres:ConnectionString' in appsettings.json");

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, param);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, param);
    }
}