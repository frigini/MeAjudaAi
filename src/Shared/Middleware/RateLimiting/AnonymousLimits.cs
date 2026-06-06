using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Middleware.RateLimiting;

[ExcludeFromCodeCoverage]

public class AnonymousLimits
{
    public int RequestsPerMinute { get; set; } = 30;
    public int RequestsPerHour { get; set; } = 300;
    public int RequestsPerDay { get; set; } = 1000;
}
