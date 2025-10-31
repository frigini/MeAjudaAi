namespace MeAjudaAi.Shared.Events;

public abstract record IntegrationEvent(
    string Source
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public string Version { get; init; } = "1.0";
}
