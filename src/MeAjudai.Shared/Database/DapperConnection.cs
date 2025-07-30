using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace MeAjudai.Shared.Database;

public class DapperConnection(IConfiguration configuration) : IDapperConnection
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

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