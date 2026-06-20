namespace MeAjudaAi.Modules.Payments.Domain.Entities;

/// <summary>
/// Representa uma mensagem na inbox para processamento assíncrono de eventos.
/// </summary>
/// <remarks>
/// Implementa o padrão Inbox para garantia de processamento至少一次 (at-least-once).
/// Mensagens são inseridas quando um evento é recebido e processadas por background jobs.
/// Suporta retry com backoff exponencial e controle de máximo de tentativas.
/// </remarks>
public sealed class InboxMessage
{
    /// <summary>
    /// Identificador único da mensagem.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do evento externo (ex: ID do evento Stripe). Usado para idempotência.
    /// </summary>
    public string? ExternalEventId { get; private set; }

    /// <summary>
    /// Tipo/tipo do evento (ex: "checkout.session.completed", "invoice.paid").
    /// </summary>
    public string Type { get; private set; } = null!;

    /// <summary>
    /// Conteúdo bruto do evento em formato JSON.
    /// </summary>
    public string Content { get; private set; } = null!;

    /// <summary>
    /// Data/hora em que a mensagem foi criada na inbox (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data/hora em que a mensagem foi processada com sucesso (UTC). Null indica que ainda não foi processada.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Mensagem de erro do último fracasso de processamento. Null quando processada com sucesso ou ainda não tentada.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Número de tentativas de processamento realizadas até o momento.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Número máximo de tentativas de processamento permitidas.
    /// </summary>
    public int MaxRetries { get; private set; }

    /// <summary>
    /// Data/hora da próxima tentativa de processamento (UTC). Null indica que pode ser tentada imediatamente.
    /// </summary>
    public DateTime? NextAttemptAt { get; private set; }

    private InboxMessage() { }

    /// <summary>
    /// Cria uma nova mensagem de inbox para processamento assíncrono.
    /// </summary>
    /// <param name="type">Tipo do evento (ex: "checkout.session.completed").</param>
    /// <param name="content">Conteúdo bruto do evento em JSON.</param>
    /// <param name="externalEventId">ID do evento externo para idempotência (opcional).</param>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="type"/> ou <paramref name="content"/> são nulos ou vazios.</exception>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="type"/> excede 255 caracteres ou <paramref name="content"/> não é JSON válido.</exception>
    public InboxMessage(string type, string content, string? externalEventId = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type), "Type cannot be null or empty.");
        
        var trimmedType = type.Trim();
        if (trimmedType.Length > 255)
            throw new ArgumentException("Type length cannot exceed 255 characters.", nameof(type));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ArgumentException("Content must be a valid JSON string.", nameof(content));
        }

        Id = Guid.NewGuid();
        ExternalEventId = externalEventId;
        Type = trimmedType;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        MaxRetries = 5;
    }

    /// <summary>
    /// Marca a mensagem como processada com sucesso.
    /// </summary>
    /// <param name="processedAt">Data/hora do processamento. Se null, usa DateTime.UtcNow.</param>
    public void MarkAsProcessed(DateTime? processedAt = null)
    {
        if (ProcessedAt != null) return;
        ProcessedAt = processedAt ?? DateTime.UtcNow;
        Error = null;
    }

    /// <summary>
    /// Registra uma falha de processamento e agenda nova tentativa com backoff exponencial.
    /// </summary>
    /// <param name="error">Mensagem de erro descrevendo a falha.</param>
    /// <param name="nextAttemptAt">Data/hora específica para próxima tentativa. Se null, usa backoff exponencial.</param>
    public void RecordError(string error, DateTime? nextAttemptAt = null)
    {
        Error = error;
        IncrementRetry();
        
        if (nextAttemptAt.HasValue)
        {
            NextAttemptAt = nextAttemptAt.Value;
        }
        else
        {
            CalculateNextAttempt();
        }
    }

    private void IncrementRetry()
    {
        RetryCount++;
    }

    private void CalculateNextAttempt()
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, RetryCount) * 30);
        NextAttemptAt = DateTime.UtcNow.Add(delay);
    }

    /// <summary>
    /// Indica se a mensagem deve ser retentada: não processada, dentro do limite de retries, e agendada para agora ou passado.
    /// </summary>
    public bool ShouldRetry => ProcessedAt == null && RetryCount < MaxRetries && (NextAttemptAt == null || NextAttemptAt <= DateTime.UtcNow);
}
