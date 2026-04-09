using MeAjudaAi.Modules.Communications.Application.Services;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Communications Outbox Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
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
