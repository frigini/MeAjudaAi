using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class GeneralSettings
{
    public bool Enabled { get; set; } = true;
    [Range(1, 86400)] public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = [];
    public bool EnableDetailedLogging { get; set; } = true;
    public string ErrorMessage { get; set; } = "Rate limit exceeded. Please try again later.";
}
