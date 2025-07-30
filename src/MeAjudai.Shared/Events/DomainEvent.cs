namespace MeAjudaAi.Shared.Events;

public abstract record DomainEvent(
    Guid AggregateId,
    int Version
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}