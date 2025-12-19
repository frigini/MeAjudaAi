using MeAjudaAi.ApiService.Options.RateLimit;

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
