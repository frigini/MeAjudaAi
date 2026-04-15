namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public DateTime? NextAttemptAt { get; set; }

    public InboxMessage() { }

    public InboxMessage(string type, string content)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type), "Type cannot be null or empty.");
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");

        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public bool ShouldRetry => ProcessedAt == null && RetryCount < MaxRetries && (NextAttemptAt == null || NextAttemptAt <= DateTime.UtcNow);
}
