using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Options;

/// <summary>
/// Configurações específicas de DLQ para RabbitMQ
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RabbitMqDeadLetterOptions
{
    /// <summary>
    /// Exchange de dead letter padrão
    /// </summary>
    public string DeadLetterExchange { get; set; } = "dlx.meajudaai";

    /// <summary>
    /// Routing key para mensagens de dead letter
    /// </summary>
    public string DeadLetterRoutingKey { get; set; } = "deadletter";

    /// <summary>
    /// Habilita DLX (Dead Letter Exchange) automático para todas as filas
    /// </summary>
    public bool EnableAutomaticDlx { get; set; } = true;

    /// <summary>
    /// Habilita persistência de mensagens na DLQ
    /// </summary>
    public bool EnablePersistence { get; set; } = true;
}
