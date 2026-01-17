using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Serviço em background para coletar métricas periódicas
/// </summary>
internal class MetricsCollectorService(
    BusinessMetrics businessMetrics,
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
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
            catch (OperationCanceledException ex)
            {
                // Expected when service is stopping
                logger.LogInformation(ex, "Metrics collection cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error collecting metrics");
            }

            try
            {
                await timeProvider.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                // Expected when service is stopping during delay
                logger.LogInformation(ex, "Metrics collection cancelled during delay");
                break;
            }
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
        catch (OperationCanceledException)
        {
            throw; // Propagate cancellation to ExecuteAsync
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
            using var scope = serviceScopeFactory.CreateScope();
            
            // Tentar obter IUsersModuleApi - retorna 0 se módulo não estiver disponível
            var moduleTypeName = "MeAjudaAi.Modules.Users.Application.ModuleApi.IUsersModuleApi, MeAjudaAi.Modules.Users.Application";
            var moduleType = Type.GetType(moduleTypeName);
            
            if (moduleType == null)
            {
                logger.LogDebug("Users module type not found, returning 0 active users");
                return 0;
            }
            
            var usersModuleApi = scope.ServiceProvider.GetService(moduleType);
            
            if (usersModuleApi == null)
            {
                logger.LogDebug("Users module not available, returning 0 active users");
                return 0;
            }

            // Por ora retorna 0 - implementação futura chamará método real do módulo
            // var count = await usersModuleApi.GetActiveUsersCountAsync(cancellationToken);
            await timeProvider.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            
            return 0;
        }
        catch (OperationCanceledException)
        {
            throw; // Propagate cancellation
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
            // Implementação futura: injetar HelpRequestRepository
            // e contar solicitações com status Pending
            await timeProvider.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            
            // Por ora, retorna 0
            return 0;
        }
        catch (OperationCanceledException)
        {
            throw; // Propagate cancellation
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get pending help requests count");
            return 0;
        }
    }
}
