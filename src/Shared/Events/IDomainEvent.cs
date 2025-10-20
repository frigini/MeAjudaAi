namespace MeAjudaAi.Shared.Events;

public interface IDomainEvent : IEvent
{
    Guid AggregateId { get; }
    int Version { get; }
}
