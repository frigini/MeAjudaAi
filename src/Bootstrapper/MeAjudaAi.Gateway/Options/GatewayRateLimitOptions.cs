namespace MeAjudaAi.Gateway.Options;

public class GatewayRateLimitOptions
{
    public const string SectionName = "GatewayRateLimit";
    public GeneralSettings General { get; set; } = new();
    public AnonymousLimits Anonymous { get; set; } = new();
    public AuthenticatedLimits Authenticated { get; set; } = new();
}

public class GeneralSettings
{
    public bool Enabled { get; set; } = true;
    public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = [];
    public string ErrorMessage { get; set; } = "Limite de requisições excedido. Tente novamente mais tarde.";
}

public class AnonymousLimits
{
    public int RequestsPerMinute { get; set; } = 30;
    public int RequestsPerHour { get; set; } = 300;
    public int RequestsPerDay { get; set; } = 1000;
}

public class AuthenticatedLimits
{
    public int RequestsPerMinute { get; set; } = 120;
    public int RequestsPerHour { get; set; } = 2000;
    public int RequestsPerDay { get; set; } = 10000;
}