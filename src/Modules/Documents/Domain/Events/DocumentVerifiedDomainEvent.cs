using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Documents.Domain.Events;

public record DocumentVerifiedDomainEvent(
    Guid DocumentId,
    Guid ProviderId,
    DocumentType DocumentType,
    string? OcrData) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public Guid AggregateId => DocumentId;
    public int Version => 1;
}
