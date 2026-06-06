using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Middleware.RateLimiting;

[ExcludeFromCodeCoverage]

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public GeneralSettings General { get; set; } = new();
    public AnonymousLimits Anonymous { get; set; } = new();
    public AuthenticatedLimits Authenticated { get; set; } = new();
}
