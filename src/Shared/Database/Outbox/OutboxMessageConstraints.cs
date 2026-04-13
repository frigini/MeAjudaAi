namespace MeAjudaAi.Shared.Database.Outbox;

/// <summary>
/// Constantes para restrições de banco de dados do Outbox
/// </summary>
public static class OutboxMessageConstraints
{
    /// <summary>
    /// Nome do índice de unicidade para correlation_id na tabela de outbox
    /// </summary>
    public const string CorrelationIdIndexName = "ix_outbox_messages_correlation_id";
}
