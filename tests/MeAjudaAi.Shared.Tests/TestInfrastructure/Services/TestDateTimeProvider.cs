using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Services;

/// <summary>
/// Test implementation of IDateTimeProvider for testing purposes.
/// Returns current UTC time by default.
/// </summary>
public class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime CurrentDate() => DateTime.UtcNow;
}
