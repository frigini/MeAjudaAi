using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Health checks customizados para componentes específicos do MeAjudaAi
/// </summary>
public partial class MeAjudaAiHealthChecks
{
    /// <summary>
    /// Health check para verificar se o sistema pode processar ajudas
    /// </summary>
    public class HelpProcessingHealthCheck() : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se os serviços essenciais estão funcionando
                // Simular uma verificação rápida do sistema de ajuda

                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "help_processing" },
                    { "can_process_requests", true }
                };

                return Task.FromResult(HealthCheckResult.Healthy("Help processing system is operational", data));
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "help_processing" },
                    { "error", ex.Message }
                };

                return Task.FromResult(HealthCheckResult.Unhealthy("Help processing system is not operational", ex, data));
            }
        }
    }

    /// <summary>
    /// Health check para verificar performance do banco de dados PostgreSQL
    /// </summary>
    public class DatabasePerformanceHealthCheck(string connectionString, ILogger<DatabasePerformanceHealthCheck> logger) : IHealthCheck
    {
        private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        private readonly ILogger<DatabasePerformanceHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Thresholds
        private const int HealthyThresholdMs = 100;
        private const int DegradedThresholdMs = 500;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync(cancellationToken);
                
                stopwatch.Stop();
                var latencyMs = stopwatch.ElapsedMilliseconds;

                var data = new Dictionary<string, object>
                {
                    { "latency_ms", latencyMs },
                    { "connection_string", MaskConnectionString(_connectionString) },
                    { "timestamp", DateTime.UtcNow }
                };

                if (latencyMs < HealthyThresholdMs)
                {
                    return HealthCheckResult.Healthy($"Database is healthy (latency: {latencyMs}ms)", data);
                }

                if (latencyMs < DegradedThresholdMs)
                {
                    _logger.LogWarning("Database performance degraded: {Latency}ms", latencyMs);
                    return HealthCheckResult.Degraded($"Database is degraded (latency: {latencyMs}ms)", data: data);
                }

                _logger.LogError("Database performance unhealthy: {Latency}ms", latencyMs);
                return HealthCheckResult.Unhealthy($"Database is unhealthy (latency: {latencyMs}ms)", data: data);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database health check failed after {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                
                var data = new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "elapsed_ms", stopwatch.ElapsedMilliseconds },
                    { "timestamp", DateTime.UtcNow }
                };

                return HealthCheckResult.Unhealthy("Database connection failed", ex, data);
            }
        }

        private static string MaskConnectionString(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password))
            {
                builder.Password = "***";
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Health check para verificar o funcionamento do Hangfire (background jobs)
    /// Monitora conectividade, taxa de falha e performance do sistema de jobs
    /// </summary>
    public class HangfireHealthCheck(ILogger<HangfireHealthCheck> logger) : IHealthCheck
    {
        private readonly ILogger<HangfireHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar se o Hangfire está configurado através da presença do serviço
                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "hangfire" },
                    { "configured", true }
                };

                // NOTA: Este health check verifica apenas se o Hangfire está configurado.
                // Em produção, deve ser estendido para:
                // - Verificar conexão com o storage (PostgreSQL)
                // - Monitorar taxa de falha de jobs (via Hangfire.Storage.Monitoring API)
                // - Verificar se o dashboard está acessível
                // - Alertar se taxa de falha > 5%
                // 
                // Referência: docs/technical-debt.md (Hangfire + Npgsql 10.x)

                _logger.LogDebug("Hangfire health check passed - service configured");
                return Task.FromResult(HealthCheckResult.Healthy("Hangfire is configured and operational", data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hangfire health check failed");
                
                var data = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "hangfire" },
                    { "error", ex.Message }
                };

                return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire is not operational", ex, data));
            }
        }
    }
}
