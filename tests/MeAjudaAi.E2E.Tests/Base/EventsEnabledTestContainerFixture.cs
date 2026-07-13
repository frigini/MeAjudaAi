namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Fixture que habilita SynchronousInMemoryMessageBus e DomainEventProcessor
/// para testes que dependem de eventos de integração entre módulos.
/// </summary>
public class EventsEnabledTestContainerFixture : TestContainerFixture
{
    public override bool EnableEventsAndMessageBus => true;
}
