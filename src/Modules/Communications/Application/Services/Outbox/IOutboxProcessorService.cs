namespace MeAjudaAi.Modules.Communications.Application.Services.Outbox;

public interface IOutboxProcessorService
{
    Task<int> ProcessPendingMessagesAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default);
}