using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Serviço em background para coletar métricas periódicas
/// </summary>
internal class MetricsCollectorService(
    BusinessMetrics businessMetrics,
    IServiceProvider serviceProvider,
    ILogger<MetricsCollectorService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Coleta a cada minuto

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Metrics collector service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectMetrics(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error collecting metrics");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        logger.LogInformation("Metrics collector service stopped");
    }

    private async Task CollectMetrics(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            // Coletar métricas de usuários ativos
            var activeUsers = await GetActiveUsersCount(scope);
            businessMetrics.UpdateActiveUsers(activeUsers);

            // Coletar métricas de solicitações pendentes
            var pendingRequests = await GetPendingHelpRequestsCount(scope);
            businessMetrics.UpdatePendingHelpRequests(pendingRequests);

            logger.LogDebug("Metrics collected: {ActiveUsers} active users, {PendingRequests} pending requests",
                activeUsers, pendingRequests);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to collect some metrics");
        }
    }

    private async Task<long> GetActiveUsersCount(IServiceScope scope)
    {
        try
        {
            // Aqui você implementaria a lógica real para contar usuários ativos
            // Por exemplo, usuários que fizeram login nas últimas 24 horas

            // Placeholder - implementar com o serviço real de usuários
            await Task.Delay(1, CancellationToken.None); // Simular operação async
            return Random.Shared.Next(50, 200); // Valor simulado
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get active users count");
            return 0;
        }
    }

    private async Task<long> GetPendingHelpRequestsCount(IServiceScope scope)
    {
        try
        {
            // Aqui você implementaria a lógica real para contar solicitações pendentes

            // Placeholder - implementar com o serviço real de help requests
            await Task.Delay(1, CancellationToken.None); // Simular operação async
            return Random.Shared.Next(0, 50); // Valor simulado
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get pending help requests count");
            return 0;
        }
    }
}