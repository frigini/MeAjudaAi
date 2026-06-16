namespace MeAjudaAi.Modules.Communications.Application.Services.Outbox;

/// <summary>
/// Serviço responsável pelo processamento de mensagens pendentes na caixa de saída (outbox).
/// </summary>
public interface IOutboxProcessorService
{
    /// <summary>
    /// Processa um lote de mensagens pendentes na caixa de saída.
    /// </summary>
    /// <param name="batchSize">Tamanho máximo do lote de mensagens a serem processadas (padrão: 50).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O número de mensagens processadas com sucesso.</returns>
    Task<int> ProcessPendingMessagesAsync(
        int batchSize = 50,
        CancellationToken cancellationToken = default);
}