using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.ApiService.Options;

/// <summary>
/// Opções para Rate Limiting com suporte a usuários autenticados.
/// </summary>
public class RateLimitOptions
{
    public const string SectionName = "AdvancedRateLimit";

    /// <summary>
    /// Configurações para usuários anônimos (não autenticados).
    /// </summary>
    public AnonymousLimits Anonymous { get; set; } = new();

    /// <summary>
    /// Configurações para usuários autenticados.
    /// </summary>
    public AuthenticatedLimits Authenticated { get; set; } = new();

    /// <summary>
    /// Configurações específicas por endpoint.
    /// </summary>
    public Dictionary<string, EndpointLimits> EndpointLimits { get; set; } = new();

    /// <summary>
    /// Configurações por role/função do usuário.
    /// </summary>
    public Dictionary<string, RoleLimits> RoleLimits { get; set; } = new();

    /// <summary>
    /// Configurações gerais.
    /// </summary>
    public GeneralSettings General { get; set; } = new();
}

public class AnonymousLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 30;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 300;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 1000;
}

public class AuthenticatedLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 120;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 2000;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 10000;
}

public class EndpointLimits
{
    [Required] public string Pattern { get; set; } = string.Empty; // supports * wildcard
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 60;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 1000;
    public bool ApplyToAuthenticated { get; set; } = true;
    public bool ApplyToAnonymous { get; set; } = true;
}

public class RoleLimits
{
    [Range(1, int.MaxValue)] public int RequestsPerMinute { get; set; } = 200;
    [Range(1, int.MaxValue)] public int RequestsPerHour { get; set; } = 5000;
    [Range(1, int.MaxValue)] public int RequestsPerDay { get; set; } = 20000;
}

public class GeneralSettings
{
    [Range(1, 86400)] public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = [];
    public bool EnableDetailedLogging { get; set; } = true;
    public string ErrorMessage { get; set; } = "Rate limit exceeded. Please try again later.";
}