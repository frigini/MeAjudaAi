namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Estatísticas das Dead Letter Queues
/// </summary>
public sealed class DeadLetterStatistics
{
    /// <summary>
    /// Total de mensagens em todas as DLQs
    /// </summary>
    public int TotalDeadLetterMessages { get; set; }

    /// <summary>
    /// Mensagens por fila de dead letter
    /// </summary>
    public Dictionary<string, int> MessagesByQueue { get; set; } = new();

    /// <summary>
    /// Mensagens por tipo de exceção
    /// </summary>
    public Dictionary<string, int> MessagesByExceptionType { get; set; } = new();

    /// <summary>
    /// Mensagens mais antigas em cada fila
    /// </summary>
    public Dictionary<string, DateTime> OldestMessageByQueue { get; set; } = new();

    /// <summary>
    /// Taxa de falha por handler
    /// </summary>
    public Dictionary<string, FailureRate> FailureRateByHandler { get; set; } = new();

    /// <summary>
    /// Data/hora da última atualização das estatísticas
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
