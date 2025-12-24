using Microsoft.Extensions.Time.Testing;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Services;

/// <summary>
/// Extension methods for FakeTimeProvider to simplify test setup.
/// </summary>
public static class FakeTimeProviderExtensions
{
    /// <summary>
    /// Creates a FakeTimeProvider with a fixed UTC date/time.
    /// </summary>
    public static FakeTimeProvider CreateFixed(DateTime utcDateTime)
    {
        return new FakeTimeProvider(new DateTimeOffset(utcDateTime, TimeSpan.Zero));
    }

    /// <summary>
    /// Creates a FakeTimeProvider with current UTC time.
    /// </summary>
    public static FakeTimeProvider CreateDefault()
    {
        return new FakeTimeProvider(DateTimeOffset.UtcNow);
    }
}
