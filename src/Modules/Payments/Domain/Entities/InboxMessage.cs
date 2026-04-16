namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }

    private InboxMessage() { }

    public InboxMessage(string type, string content)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type), "Type cannot be null or empty.");
        
        var trimmedType = type.Trim();
        if (trimmedType.Length > 255)
            throw new ArgumentException("Type length cannot exceed 255 characters.", nameof(type));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");

        // Validar se o conteúdo é um JSON válido
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new ArgumentException("Content must be a valid JSON string.", nameof(content));
        }

        Id = Guid.NewGuid();
        Type = trimmedType;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        MaxRetries = 5;
    }

    public void MarkAsProcessed(DateTime? processedAt = null)
    {
        if (ProcessedAt != null) return;
        ProcessedAt = processedAt ?? DateTime.UtcNow;
        Error = null;
    }

    public void RecordError(string error, DateTime? nextAttemptAt = null)
    {
        Error = error;
        if (nextAttemptAt.HasValue)
        {
            NextAttemptAt = nextAttemptAt.Value;
        }
        else
        {
            CalculateNextAttempt();
        }
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }

    private void CalculateNextAttempt()
    {
        // Backoff exponencial simples: 2^retry * 30 segundos
        var delay = TimeSpan.FromSeconds(Math.Pow(2, RetryCount) * 30);
        NextAttemptAt = DateTime.UtcNow.Add(delay);
    }

    public bool ShouldRetry => ProcessedAt == null && RetryCount < MaxRetries && (NextAttemptAt == null || NextAttemptAt <= DateTime.UtcNow);
}
