namespace MeAjudai.Shared.Events;

public interface IIntegrationEvent : IEvent
{
    string Source { get; }
}