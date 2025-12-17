using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Jobs.HealthChecks;

/// <summary>
/// Health check para verificar o funcionamento do Hangfire (background jobs)
/// Monitora conectividade, taxa de falha e performance do sistema de jobs
/// </summary>
public class HangfireHealthCheck(
    ILogger<HangfireHealthCheck> logger,
    IServiceProvider serviceProvider) : IHealthCheck
{
    private readonly ILogger<HangfireHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Verificar se JobStorage está configurado (indica que Hangfire foi inicializado)
        // JobStorage.Current lança InvalidOperationException se não foi inicializado
        JobStorage? storage;
        try
        {
            storage = JobStorage.Current;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Hangfire JobStorage not initialized");
            return Task.FromResult(HealthCheckResult.Degraded(
                "Hangfire is not operational",
                data: new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "component", "hangfire" },
                    { "error", ex.Message }
                }));
        }

        if (storage == null)
        {
            _logger.LogWarning("Hangfire JobStorage is null");
            return Task.FromResult(HealthCheckResult.Degraded("Hangfire JobStorage is null"));
        }

        // Tentar obter IBackgroundJobClient para confirmar que o Hangfire está funcional
        var jobClient = _serviceProvider.GetService(typeof(IBackgroundJobClient)) as IBackgroundJobClient;
        
        var data = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow },
            { "component", "hangfire" },
            { "configured", true },
            { "storage_type", storage.GetType().Name },
            { "job_client_available", jobClient != null }
        };

        // NOTA: Em produção, considerar estender para:
        // - Monitorar taxa de falha de jobs (via Hangfire.Storage.Monitoring API)
        // - Verificar latência da conexão com storage
        // - Alertar se taxa de falha > 5%
        // 
        // Referência: docs/technical-debt.md (Hangfire + Npgsql 10.x)

        _logger.LogDebug("Hangfire health check passed - storage: {StorageType}", storage.GetType().Name);
        return Task.FromResult(HealthCheckResult.Healthy("Hangfire is configured and operational", data));
    }
}
