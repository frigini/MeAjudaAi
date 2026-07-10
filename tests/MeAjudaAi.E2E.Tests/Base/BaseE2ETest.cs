namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Base class for E2E tests that eliminates boilerplate: IAsyncLifetime, constructor, InitializeAsync, DisposeAsync.
/// 
/// Usage:
///   public class MyTests : BaseE2ETest&lt;TestContainerFixture&gt; { ... }
///   public class MyEventsTests : BaseE2ETest&lt;EventsEnabledTestContainerFixture&gt; { ... }
/// 
/// For tests that need extra parameters (e.g., ITestOutputHelper), use the base directly:
///   public class MyTests(EventsEnabledTestContainerFixture fixture, ITestOutputHelper output) 
///       : BaseE2ETest&lt;EventsEnabledTestContainerFixture&gt;(fixture)
///   {
///       private readonly ITestOutputHelper _output = output;
///   }
/// </summary>
public abstract class BaseE2ETest<TFixture>(TFixture fixture) : IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : TestContainerFixture
{
    protected TFixture Fixture { get; } = fixture;

    /// <summary>
    /// Override to control whether CleanupDatabaseAsync runs before each test.
    /// Defaults to true. Set to false for tests that manage their own state.
    /// </summary>
    protected virtual bool CleanupBeforeEachTest => true;

    public async ValueTask InitializeAsync()
    {
        await Fixture.InitializeAsync();
        if (CleanupBeforeEachTest)
            await Fixture.CleanupDatabaseAsync();
    }

    public ValueTask DisposeAsync() => default;
}

/// <summary>
/// Convenience base for E2E tests that need the synchronous in-memory message bus
/// and domain event processing (events integration between modules).
/// </summary>
public abstract class BaseEventsE2ETest(EventsEnabledTestContainerFixture fixture) : BaseE2ETest<EventsEnabledTestContainerFixture>(fixture)
{
}
