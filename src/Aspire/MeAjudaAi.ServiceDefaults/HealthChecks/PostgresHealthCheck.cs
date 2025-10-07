using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MeAjudaAi.ServiceDefaults.HealthChecks;

public sealed class PostgresHealthCheck(PostgresOptions postgresOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(postgresOptions.ConnectionString))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection string not configured");
        }

        try
        {
            using var connection = new NpgsqlConnection(postgresOptions.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is not responsive", ex);
        }
    }
}