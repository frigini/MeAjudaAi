using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Middleware.RateLimiting;

[ExcludeFromCodeCoverage]

public class RateLimitCounter
{
    private int _value;

    public int Value => _value;

    public int IncrementAndGet() => Interlocked.Increment(ref _value);
}
