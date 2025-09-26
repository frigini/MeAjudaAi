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
    /// Indica se deve permitir credenciais em requisições CORS.
    /// Padrão é false por segurança.
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// Tempo máximo do cache do preflight em segundos.
    /// Padrão é 1 hora (3600 segundos).
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

        if (PreflightMaxAge < 0)
            throw new InvalidOperationException("PreflightMaxAge must be non-negative.");

        // Validação do formato das origens
        foreach (var origin in AllowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new InvalidOperationException("CORS allowed origins cannot contain empty values.");

            if (origin != "*" && !Uri.TryCreate(origin, UriKind.Absolute, out _))
                throw new InvalidOperationException($"Invalid CORS origin format: {origin}");
        }

        // Validação de segurança: alerta se usar coringa em ambientes de produção
        if (AllowedOrigins.Contains("*") && AllowCredentials)
            throw new InvalidOperationException("Cannot use wildcard origin (*) with credentials enabled for security reasons.");
    }
}
