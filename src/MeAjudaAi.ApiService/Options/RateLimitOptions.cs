namespace MeAjudaAi.ApiService.Options;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int DefaultRequestsPerMinute { get; set; } = 60;
    public int AuthRequestsPerMinute { get; set; } = 5;
    public int SearchRequestsPerMinute { get; set; } = 100;
    public int WindowInSeconds { get; set; } = 60;
}