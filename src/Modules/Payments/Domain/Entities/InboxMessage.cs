namespace MeAjudaAi.Modules.Payments.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public DateTime? NextAttemptAt { get; set; }

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
    }

    public bool ShouldRetry => ProcessedAt == null && RetryCount < MaxRetries && (NextAttemptAt == null || NextAttemptAt <= DateTime.UtcNow);
}
