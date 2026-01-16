namespace MeAjudaAi.Web.Admin.Features.Errors;

/// <summary>
/// Global error state for Fluxor
/// </summary>
public record ErrorState
{
    /// <summary>
    /// Current global error (component render errors, unhandled exceptions)
    /// </summary>
    public Exception? GlobalError { get; init; }

    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// User-friendly error message
    /// </summary>
    public string? UserMessage { get; init; }

    /// <summary>
    /// Technical error details (visible only in debug mode)
    /// </summary>
    public string? TechnicalDetails { get; init; }

    /// <summary>
    /// Whether to show error UI
    /// </summary>
    public bool ShowErrorUI { get; init; }

    /// <summary>
    /// Timestamp when error occurred
    /// </summary>
    public DateTimeOffset? OccurredAt { get; init; }

    /// <summary>
    /// Component name where error occurred
    /// </summary>
    public string? ComponentName { get; init; }

    /// <summary>
    /// Whether error is recoverable (user can retry)
    /// </summary>
    public bool IsRecoverable { get; init; }
}
