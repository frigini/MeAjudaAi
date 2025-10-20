namespace MeAjudaAi.Shared.Events;

public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}
