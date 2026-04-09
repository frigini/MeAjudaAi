namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// DTO de log de comunicação.
/// </summary>
public sealed record CommunicationLogDto(
    Guid Id,
    string CorrelationId,
    string Channel,
    string Recipient,
    string? TemplateKey,
    bool IsSuccess,
    string? ErrorMessage,
    int AttemptCount,
    DateTime SentAt
);
