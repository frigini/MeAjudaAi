using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;

/// <summary>
/// Fixture simplificado que cria containers individuais - mais confiável para CI
/// Inclui PostgreSQL com PostGIS para testes determinísticos
/// </summary>
public sealed class SimpleDatabaseFixture : IAsyncLifetime
{
    private static PostgreSqlContainer? _postgresContainer;
    private static readonly SemaphoreSlim _initializationLock = new(1, 1);
    private static bool _initialized = false;

    /// <summary>
    /// Connection string com detalhes de erro habilitados para diagnóstico em CI
    /// </summary>
    public string GetConnectionString(string databaseName)
    {
        if (_postgresContainer == null) throw new InvalidOperationException("Postgres container not initialized");

        var builder = new NpgsqlConnectionStringBuilder(_postgresContainer.GetConnectionString())
        {
            Database = databaseName,
            IncludeErrorDetail = true
        };

        return builder.ConnectionString;
    }

    public string? ConnectionString => _postgresContainer?.GetConnectionString();

    public async Task CreateDatabaseAsync(string databaseName)
    {
        if (_postgresContainer == null) throw new InvalidOperationException("Postgres container not initialized");

        if (string.IsNullOrWhiteSpace(databaseName) || !System.Text.RegularExpressions.Regex.IsMatch(databaseName, @"^[A-Za-z0-9_]+$"))
            throw new ArgumentException("Invalid database name format. Only letters, numbers and underscores allowed.", nameof(databaseName));

        var masterConnectionString = _postgresContainer.GetConnectionString();
        await using var conn = new NpgsqlConnection(masterConnectionString);
        await conn.OpenAsync();

        await using var checkCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", conn);
        checkCmd.Parameters.AddWithValue("dbName", databaseName);
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists == null)
        {
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE {databaseName}", conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DB-FIXTURE] Database {databaseName} created");
        }
    }

    public async Task DropDatabaseAsync(string databaseName)
    {
        if (_postgresContainer == null) return;

        if (string.IsNullOrWhiteSpace(databaseName) || !System.Text.RegularExpressions.Regex.IsMatch(databaseName, @"^[A-Za-z0-9_]+$"))
            throw new ArgumentException("Invalid database name format. Only letters, numbers and underscores allowed.", nameof(databaseName));

        try
        {
            var masterConnectionString = _postgresContainer.GetConnectionString();
            await using var conn = new NpgsqlConnection(masterConnectionString);
            await conn.OpenAsync();

            await using var terminateCmd = new NpgsqlCommand($"""
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{databaseName}'
                AND pid <> pg_backend_pid();
                """, conn);
            await terminateCmd.ExecuteNonQueryAsync();

            await using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {databaseName}", conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"[DB-FIXTURE] Database {databaseName} dropped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB-FIXTURE] ERROR: Could not drop database {databaseName}: {ex.Message}");
            throw;
        }
    }

    public async ValueTask InitializeAsync()
    {
        if (_initialized) return;

        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized) return;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Console.WriteLine("[SimpleDatabaseFixture] Npgsql.EnableLegacyTimestampBehavior = true");

            _postgresContainer ??= new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("meajudaai_test")
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithCleanUp(true)
                .Build();

            await _postgresContainer.StartAsync();
            await EnsurePostGisExtensionAsync();

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task EnsurePostGisExtensionAsync()
    {
        if (_postgresContainer == null)
            return;

        try
        {
            var connectionString = $"{_postgresContainer.GetConnectionString()};Include Error Detail=true";
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis;", conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("[DB-CONTAINER] PostGIS extension verified/created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB-CONTAINER] Warning: Could not ensure PostGIS extension: {ex.Message}");
        }
    }
}
