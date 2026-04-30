namespace MeAjudaAi.Gateway.Options;

public class GatewayResilienceOptions
{
    public const string SectionName = "GatewayResilience";

    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 100;
    public List<string> RetryableMethods { get; set; } = ["GET", "HEAD", "OPTIONS"];
}