using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options;

public class CorsOptions
{
    public const string SectionName = "Cors";

    [Required]
    public List<string> AllowedOrigins { get; set; } = [];

    [Required]
    public List<string> AllowedMethods { get; set; } = [];

    [Required]
    public List<string> AllowedHeaders { get; set; } = [];

    /// <summary>
    /// Whether to allow credentials in CORS requests.
    /// Defaults to false for security.
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// Maximum age for preflight cache in seconds.
    /// Defaults to 1 hour (3600 seconds).
    /// </summary>
    public int PreflightMaxAge { get; set; } = 3600;

    public void Validate()
    {
        if (!AllowedOrigins.Any())
            throw new InvalidOperationException("At least one allowed origin must be configured for CORS.");

        if (!AllowedMethods.Any())
            throw new InvalidOperationException("At least one allowed method must be configured for CORS.");

        if (!AllowedHeaders.Any())
            throw new InvalidOperationException("At least one allowed header must be configured for CORS.");

        // Validate origins format
        foreach (var origin in AllowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new InvalidOperationException("CORS allowed origins cannot contain empty values.");

            if (origin != "*" && !Uri.TryCreate(origin, UriKind.Absolute, out _))
                throw new InvalidOperationException($"Invalid CORS origin format: {origin}");
        }

        // Security validation: warn if using wildcard in production-like settings
        if (AllowedOrigins.Contains("*") && AllowCredentials)
            throw new InvalidOperationException("Cannot use wildcard origin (*) with credentials enabled for security reasons.");
    }
}