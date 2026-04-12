using MeAjudaAi.Modules.Communications.Application.Services;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Worker de background para processamento periódico do Outbox de comunicações.
/// </summary>
internal sealed class CommunicationsOutboxWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<CommunicationsOutboxWorker> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _stuckTimeout = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Communications Outbox Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                
                // 1. Recupera mensagens travadas em processamento
                var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                var resetCount = await repository.ResetStaleProcessingMessagesAsync(DateTime.UtcNow.Subtract(_stuckTimeout), stoppingToken);

                
                if (resetCount > 0)
                {
                    logger.LogWarning("Reset {Count} stuck outbox messages back to Pending.", resetCount);
                }

                // 2. Processa mensagens pendentes
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessorService>();
                int processedCount = await processor.ProcessPendingMessagesAsync(50, stoppingToken);

                if (processedCount > 0)
                {
                    logger.LogInformation("Processed {Count} outbox messages.", processedCount);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing communications outbox.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        logger.LogInformation("Communications Outbox Worker stopped.");
    }
}
