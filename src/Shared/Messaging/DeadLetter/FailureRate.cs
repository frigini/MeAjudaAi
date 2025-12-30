namespace MeAjudaAi.Shared.Messaging.DeadLetter;

/// <summary>
/// Taxa de falha para um handler específico
/// </summary>
public sealed class FailureRate
{
    /// <summary>
    /// Número total de mensagens processadas
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// Número de mensagens que falharam
    /// </summary>
    public int FailedMessages { get; set; }

    /// <summary>
    /// Percentual de falha
    /// </summary>
    public double FailurePercentage => TotalMessages > 0 ? (double)FailedMessages / TotalMessages * 100 : 0;

    /// <summary>
    /// Última falha registrada
    /// </summary>
    public DateTime? LastFailure { get; set; }
}
