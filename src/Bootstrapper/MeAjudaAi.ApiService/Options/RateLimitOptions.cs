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
    public int RequestsPerMinute { get; set; } = 30;
    public int RequestsPerHour { get; set; } = 300;
    public int RequestsPerDay { get; set; } = 1000;
}

public class AuthenticatedLimits
{
    public int RequestsPerMinute { get; set; } = 120;
    public int RequestsPerHour { get; set; } = 2000;
    public int RequestsPerDay { get; set; } = 10000;
}

public class EndpointLimits
{
    public string Pattern { get; set; } = string.Empty;
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public bool ApplyToAuthenticated { get; set; } = true;
    public bool ApplyToAnonymous { get; set; } = true;
}

public class RoleLimits
{
    public int RequestsPerMinute { get; set; } = 200;
    public int RequestsPerHour { get; set; } = 5000;
    public int RequestsPerDay { get; set; } = 20000;
}

public class GeneralSettings
{
    public int WindowInSeconds { get; set; } = 60;
    public bool EnableIpWhitelist { get; set; } = false;
    public List<string> WhitelistedIps { get; set; } = new();
    public bool EnableDetailedLogging { get; set; } = true;
    public string ErrorMessage { get; set; } = "Rate limit exceeded. Please try again later.";
}