using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class EndpointLimits
{
    [Required, MinLength(1)] public string Pattern { get; set; } = string.Empty; // supports * wildcard
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 60;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 1000;
    public bool ApplyToAuthenticated { get; set; } = true;
    public bool ApplyToAnonymous { get; set; } = true;
}
