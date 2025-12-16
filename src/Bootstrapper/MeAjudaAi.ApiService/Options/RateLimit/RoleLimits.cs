using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options.RateLimit;

public class RoleLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 200;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 5000;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 20000;
}
