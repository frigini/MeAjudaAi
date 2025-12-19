namespace MeAjudaAi.Shared.Messaging.Options;

/// <summary>
/// Opções de configuração para Dead Letter Queue (DLQ)
/// </summary>
public sealed class DeadLetterOptions
{
    public const string SectionName = "Messaging:DeadLetter";

    /// <summary>
    /// Habilita o sistema de Dead Letter Queue
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Número máximo de tentativas antes de enviar para DLQ
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Intervalo inicial entre tentativas (em segundos)
    /// </summary>
    public int InitialRetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Fator multiplicador para backoff exponencial
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Tempo máximo de delay entre tentativas (em segundos)
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 300; // 5 minutos

    /// <summary>
    /// Time to Live para mensagens na DLQ (em horas)
    /// </summary>
    public int DeadLetterTtlHours { get; set; } = 72; // 3 dias

    /// <summary>
    /// Prefixo para nomear filas de dead letter
    /// </summary>
    public string DeadLetterQueuePrefix { get; set; } = "dlq";

    /// <summary>
    /// Habilita logging detalhado de falhas
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Habilita notificações para administradores quando mensagens são enviadas para DLQ
    /// </summary>
    public bool EnableAdminNotifications { get; set; } = true;

    /// <summary>
    /// Tipos de exceções que não devem causar retry (immediate DLQ)
    /// </summary>
    public string[] NonRetryableExceptions { get; set; } = {
        "System.ArgumentException",
        "System.ArgumentNullException",
        "System.FormatException",
        "System.InvalidOperationException",
        "MeAjudaAi.Shared.Exceptions.BusinessRuleException",
        "MeAjudaAi.Shared.Exceptions.DomainException"
    };

    /// <summary>
    /// Tipos de exceções que sempre devem causar retry
    /// </summary>
    public string[] RetryableExceptions { get; set; } = {
        "System.TimeoutException",
        "System.Net.Http.HttpRequestException",
        "Npgsql.PostgresException",
        "Microsoft.Data.SqlClient.SqlException",
        "System.Net.Sockets.SocketException"
    };

    /// <summary>
    /// Configurações específicas para RabbitMQ
    /// </summary>
    public RabbitMqDeadLetterOptions RabbitMq { get; set; } = new();

    /// <summary>
    /// Configurações específicas para Azure Service Bus
    /// </summary>
    public ServiceBusDeadLetterOptions ServiceBus { get; set; } = new();
}

/// <summary>
/// Configurações específicas de DLQ para RabbitMQ
/// </summary>
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

/// <summary>
/// Configurações específicas de DLQ para Azure Service Bus
/// </summary>
public sealed class ServiceBusDeadLetterOptions
{
    /// <summary>
    /// Sufixo para filas de dead letter no Service Bus
    /// </summary>
    public string DeadLetterQueueSuffix { get; set; } = "$DeadLetterQueue";

    /// <summary>
    /// Habilita auto-complete para mensagens processadas com sucesso
    /// </summary>
    public bool EnableAutoComplete { get; set; } = true;

    /// <summary>
    /// Tempo máximo de lock para mensagens (em minutos)
    /// </summary>
    public int MaxLockDurationMinutes { get; set; } = 5;
}
