namespace MeAjudaAi.Shared.Events;

public interface IIntegrationEvent : IEvent
{
    string Source { get; }
}
