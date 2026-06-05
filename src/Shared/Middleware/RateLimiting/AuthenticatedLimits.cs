namespace MeAjudaAi.Shared.Middleware.RateLimiting;

public class AuthenticatedLimits
{
    public int RequestsPerMinute { get; set; } = 120;
    public int RequestsPerHour { get; set; } = 2000;
    public int RequestsPerDay { get; set; } = 10000;
}
