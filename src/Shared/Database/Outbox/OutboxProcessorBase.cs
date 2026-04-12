using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Classe base para processadores de Outbox.
/// Centraliza lógica de polling, retries e atualização de estado.
/// </summary>
/// <typeparam name="TMessage">Tipo da entidade de mensagem.</typeparam>
public abstract class OutboxProcessorBase<TMessage>(
    IOutboxRepository<TMessage> outboxRepository,
    ILogger logger)
    where TMessage : OutboxMessage
{
    /// <summary>
    /// Processa um lote de mensagens pendentes.
    /// </summary>
    /// <param name="batchSize">Tamanho do lote.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Número de mensagens processadas com sucesso.</returns>
    public virtual async Task<int> ProcessPendingMessagesAsync(
        int batchSize = 20,
        CancellationToken cancellationToken = default)
    {
        var messages = await outboxRepository.GetPendingAsync(batchSize, DateTime.UtcNow, cancellationToken);
        if (messages.Count == 0) return 0;

        logger.LogInformation("Processing {Count} pending outbox messages...", messages.Count);

        var processed = 0;
        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;

            message.MarkAsProcessing();
            await outboxRepository.SaveChangesAsync(cancellationToken);

            try
            {
                var result = await DispatchAsync(message, cancellationToken);

                if (result.IsCanceled)
                {
                    message.ResetToPending();
                    await outboxRepository.SaveChangesAsync(cancellationToken);
                    break;
                }

                if (result.IsSuccess)
                {
                    message.MarkAsSent(DateTime.UtcNow);
                    await OnSuccessAsync(message, cancellationToken);
                    processed++;
                }
                else
                {
                    message.MarkAsFailed(result.ErrorMessage ?? "Dispatch failed without error message.");
                    await OnFailureAsync(message, result.ErrorMessage, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                message.ResetToPending();
                await outboxRepository.SaveChangesAsync(CancellationToken.None); // Force save status reset
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {Id}: {Message}", message.Id, ex.Message);
                message.MarkAsFailed(ex.Message);
                await OnFailureAsync(message, ex.Message, cancellationToken);
            }

            await outboxRepository.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }

    /// <summary>
    /// Implementação do despacho real da mensagem (deve ser sobrescrito pelo módulo).
    /// </summary>
    protected abstract Task<DispatchResult> DispatchAsync(TMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Gancho para execução após sucesso (ex: logging de auditoria).
    /// </summary>
    protected virtual Task OnSuccessAsync(TMessage message, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Gancho para execução após falha (ex: logging de erro).
    /// </summary>
    protected virtual Task OnFailureAsync(TMessage message, string? error, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Resultado de um despacho de mensagem.
    /// </summary>
    public record DispatchResult(bool IsSuccess, string? ErrorMessage = null, bool IsCanceled = false)
    {
        public static DispatchResult Success() => new(true);
        public static DispatchResult Failure(string errorMessage) => new(false, errorMessage);
        public static DispatchResult Canceled() => new(false, null, true);
    }
}
