namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Variante da TestContainerFixture que habilita o SynchronousInMemoryMessageBus e DomainEventProcessor reais.
/// Usada por testes que dependem de processamento de eventos de integração entre módulos
/// (ex: Ratings → SearchProviders, Bookings → Providers).
/// </summary>
public class TestContainerFixtureWithEvents : TestContainerFixture
{
    public override bool EnableEventsAndMessageBus => true;
}
