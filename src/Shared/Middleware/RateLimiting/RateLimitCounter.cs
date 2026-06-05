namespace MeAjudaAi.Shared.Middleware.RateLimiting;

public class RateLimitCounter
{
    private int _value;

    public int Value => _value;

    public int IncrementAndGet() => Interlocked.Increment(ref _value);
}
