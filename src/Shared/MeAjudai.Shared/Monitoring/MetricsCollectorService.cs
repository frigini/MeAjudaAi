using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Serviço em background para coletar métricas periódicas
/// </summary>
public class MetricsCollectorService : BackgroundService
{
    private readonly BusinessMetrics _businessMetrics;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsCollectorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Coleta a cada minuto

    public MetricsCollectorService(
        BusinessMetrics businessMetrics,
        IServiceProvider serviceProvider,
        ILogger<MetricsCollectorService> logger)
    {
        _businessMetrics = businessMetrics;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics collector service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectMetrics(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting metrics");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Metrics collector service stopped");
    }

    private async Task CollectMetrics(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            // Coletar métricas de usuários ativos
            var activeUsers = await GetActiveUsersCount(scope);
            _businessMetrics.UpdateActiveUsers(activeUsers);

            // Coletar métricas de solicitações pendentes
            var pendingRequests = await GetPendingHelpRequestsCount(scope);
            _businessMetrics.UpdatePendingHelpRequests(pendingRequests);

            _logger.LogDebug("Metrics collected: {ActiveUsers} active users, {PendingRequests} pending requests",
                activeUsers, pendingRequests);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect some metrics");
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
            _logger.LogWarning(ex, "Failed to get active users count");
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
            _logger.LogWarning(ex, "Failed to get pending help requests count");
            return 0;
        }
    }
}

/// <summary>
/// Extension methods para registrar o serviço de coleta de métricas
/// </summary>
public static class MetricsCollectorExtensions
{
    /// <summary>
    /// Adiciona o serviço de coleta de métricas
    /// </summary>
    public static IServiceCollection AddMetricsCollector(this IServiceCollection services)
    {
        return services.AddHostedService<MetricsCollectorService>();
    }
}