using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.HealthChecks;

/// <summary>
/// Health check específico para o módulo Providers
/// Verifica se o banco de dados e as tabelas estão acessíveis
/// </summary>
public class ProvidersHealthCheck(
    ProvidersDbContext dbContext,
    ILogger<ProvidersHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifica se consegue conectar ao banco
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to providers database");
            }

            // Verifica se consegue acessar a tabela providers
            var providersCount = await dbContext.Providers.CountAsync(cancellationToken);
            
            logger.LogDebug("Providers health check passed - {ProvidersCount} providers found", providersCount);
            
            return HealthCheckResult.Healthy($"Providers database is accessible with {providersCount} providers");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Providers health check failed");
            return HealthCheckResult.Unhealthy("Providers database health check failed", ex);
        }
    }
}