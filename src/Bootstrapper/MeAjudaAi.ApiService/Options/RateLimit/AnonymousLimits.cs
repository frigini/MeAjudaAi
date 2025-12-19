using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class AnonymousLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 30;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 300;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 1000;
}
