namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public DateTime? NextAttemptAt { get; set; }

    public bool ShouldRetry => ProcessedAt == null && RetryCount < MaxRetries && (NextAttemptAt == null || NextAttemptAt <= DateTime.UtcNow);
}
