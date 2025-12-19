using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class AuthenticatedLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 120;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 2000;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 10000;
}
