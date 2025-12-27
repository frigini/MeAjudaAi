using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Tests.Messaging;

/// <summary>
/// Evento de teste para uso em testes unitários
/// </summary>
public class TestEvent : IEvent
{
    public string EventType => "TestEvent";
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
