using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class RateLimitOptions
{
    public GeneralSettings General { get; set; } = new();
    public AnonymousLimits Anonymous { get; set; } = new();
    public AuthenticatedLimits Authenticated { get; set; } = new();
    public Dictionary<string, RoleLimits> RoleLimits { get; set; } = new();
    public Dictionary<string, EndpointLimits> EndpointLimits { get; set; } = new();
}