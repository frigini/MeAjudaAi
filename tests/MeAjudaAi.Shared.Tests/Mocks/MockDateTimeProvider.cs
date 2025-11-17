using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Tests.Mocks;

/// <summary>
/// Mock implementation of IDateTimeProvider for testing purposes.
/// Allows controlling the current date/time in tests.
/// </summary>
public sealed class MockDateTimeProvider : IDateTimeProvider
{
    private DateTime? _fixedDateTime;

    /// <summary>
    /// Creates a new instance with the current UTC time.
    /// </summary>
    public MockDateTimeProvider()
    {
        _fixedDateTime = null;
    }

    /// <summary>
    /// Creates a new instance with a fixed date/time.
    /// </summary>
    /// <param name="fixedDateTime">The fixed date/time to return</param>
    public MockDateTimeProvider(DateTime fixedDateTime)
    {
        _fixedDateTime = fixedDateTime;
    }

    /// <summary>
    /// Gets the current date/time.
    /// Returns the fixed date/time if set, otherwise returns the current UTC time.
    /// </summary>
    public DateTime CurrentDate() => _fixedDateTime ?? DateTime.UtcNow;

    /// <summary>
    /// Sets a fixed date/time to be returned by CurrentDate().
    /// </summary>
    /// <param name="dateTime">The fixed date/time to use</param>
    public void SetFixedDateTime(DateTime dateTime)
    {
        _fixedDateTime = dateTime;
    }

    /// <summary>
    /// Resets to use the current UTC time instead of a fixed value.
    /// </summary>
    public void Reset()
    {
        _fixedDateTime = null;
    }

    /// <summary>
    /// Advances the fixed date/time by the specified duration.
    /// If no fixed date/time is set, initializes to current UTC time first.
    /// Note: This may introduce non-deterministic behavior if the exact starting time matters for your test.
    /// Consider using SetFixedDateTime() or the constructor overload to explicitly set a starting time.
    /// </summary>
    /// <param name="duration">The duration to advance</param>
    public void Advance(TimeSpan duration)
    {
        _fixedDateTime = (_fixedDateTime ?? DateTime.UtcNow).Add(duration);
    }
}
