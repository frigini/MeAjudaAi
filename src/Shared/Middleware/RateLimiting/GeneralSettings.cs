using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Middleware.RateLimiting;

[ExcludeFromCodeCoverage]

public class GeneralSettings
{
    public bool Enabled { get; set; } = true;
    public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = [];
    public string ErrorMessage { get; set; } = "Limite de requisições excedido. Tente novamente mais tarde.";
}
