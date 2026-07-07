namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Fixture for E2E tests that require the synchronous in-memory message bus
/// and domain event processing (events integration between modules).
/// </summary>
public class EventsEnabledTestContainerFixture : TestContainerFixture
{
    public override bool EnableEventsAndMessageBus => true;
}
