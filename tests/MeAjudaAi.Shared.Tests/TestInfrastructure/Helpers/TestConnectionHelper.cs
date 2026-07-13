using Npgsql;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;

/// <summary>
/// Helper para obter connection strings de teste com fallback entre Aspire e variáveis de ambiente
/// </summary>
public static class TestConnectionHelper
{
    /// <summary>
    /// Obtém connection string com prioridade: Aspire > Environment Variables > Defaults
    /// </summary>
    public static string GetConnectionString()
    {
        var aspireConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__postgresdb");

        if (!string.IsNullOrWhiteSpace(aspireConnectionString))
        {
            return aspireConnectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_HOST") ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PORT"), out var port) ? port : 5432,
            Database = Environment.GetEnvironmentVariable("MEAJUDAAI_DB") ?? "meajudaai_tests",
            Username = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_USER") ?? "postgres",
            Password = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PASS") ?? "postgres"
        };

        return builder.ConnectionString;
    }
}
