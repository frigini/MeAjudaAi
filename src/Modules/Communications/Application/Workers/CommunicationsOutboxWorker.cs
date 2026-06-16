using MeAjudaAi.Modules.Communications.Application.Services.Outbox;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace MeAjudaAi.Modules.Communications.Application.Workers;

/// <summary>
/// Worker de background para processamento periódico do Outbox de comunicações.
/// </summary>
internal sealed class CommunicationsOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CommunicationsOutboxWorker> _logger;
    private readonly TimeSpan _checkInterval;

    public CommunicationsOutboxWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CommunicationsOutboxWorker> logger,
        TimeSpan? checkInterval = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        var interval = checkInterval ?? TimeSpan.FromSeconds(10);
        if (interval <= TimeSpan.Zero && interval != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(checkInterval), "Check interval must be greater than zero or InfiniteTimeSpan.");
        }
        
        _checkInterval = interval;
    }

    private readonly TimeSpan _stuckTimeout = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Communications Outbox Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                
                // 1. Recupera mensagens travadas em processamento
                var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
                var resetCount = await repository.ResetStaleProcessingMessagesAsync(DateTime.UtcNow.Subtract(_stuckTimeout), stoppingToken);

                
                if (resetCount > 0)
                {
                    _logger.LogWarning("Reset {Count} stuck outbox messages back to Pending.", resetCount);
                }

                // 2. Processa mensagens pendentes
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessorService>();
                int processedCount = await processor.ProcessPendingMessagesAsync(50, stoppingToken);

                if (processedCount > 0)
                {
                    _logger.LogInformation("Processed {Count} outbox messages.", processedCount);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                // Rethrow truly fatal exceptions so the process can fail fast,
                // handle/log only non-fatal exceptions to avoid swallowing critical errors (satisfies CA1031).
                if (IsFatal(ex))
                {
                    _logger.LogCritical(ex, "Fatal exception occurred in communications outbox worker; rethrowing.");
                    throw;
                }

                _logger.LogError(ex, "Error occurred while processing communications outbox.");
                
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Exit loop
                }
            }
        }

        _logger.LogInformation("Communications Outbox Worker stopped.");
    }

    private static bool IsFatal(Exception ex) =>
        ex is OutOfMemoryException
        or StackOverflowException
        or ThreadAbortException
        or AccessViolationException
        or SEHException
        or AppDomainUnloadedException;
}
