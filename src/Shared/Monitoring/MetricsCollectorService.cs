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
        try
        {
            // Coletar métricas de usuários ativos
            var activeUsers = await GetActiveUsersCount(cancellationToken);
            businessMetrics.UpdateActiveUsers(activeUsers);

            // Coletar métricas de solicitações pendentes
            var pendingRequests = await GetPendingHelpRequestsCount(cancellationToken);
            businessMetrics.UpdatePendingHelpRequests(pendingRequests);

            logger.LogDebug("Metrics collected: {ActiveUsers} active users, {PendingRequests} pending requests",
                activeUsers, pendingRequests);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to collect some metrics");
        }
    }

    private async Task<long> GetActiveUsersCount(CancellationToken cancellationToken)
    {
        try
        {
            // Aqui você implementaria a lógica real para contar usuários ativos
            // Por exemplo, usuários que fizeram login nas últimas 24 horas
            // TODO: Quando implementar, usar IServiceScope para resolver DbContext/repositories

            // Placeholder - implementar com o serviço real de usuários
            await Task.Delay(1, cancellationToken); // Simular operação async

            // TODO: Implementar lógica real - por ora retorna valor fixo para evitar Random inseguro
            return 125; // Valor simulado fixo
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get active users count");
            return 0;
        }
    }

    private async Task<long> GetPendingHelpRequestsCount(CancellationToken cancellationToken)
    {
        try
        {
            // Aqui você implementaria a lógica real para contar solicitações pendentes
            // TODO: Quando implementar, usar IServiceScope para resolver DbContext/repositories

            // Placeholder - implementar com o serviço real de help requests
            await Task.Delay(1, cancellationToken); // Simular operação async

            // TODO: Implementar lógica real - por ora retorna valor fixo para evitar Random inseguro
            return 25; // Valor simulado fixo
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get pending help requests count");
            return 0;
        }
    }
}
