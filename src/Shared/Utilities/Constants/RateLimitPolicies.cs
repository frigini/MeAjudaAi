namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de políticas de rate limiting
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Política para endpoints públicos anonimizados
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Política para registro de prestadores
    /// </summary>
    public const string ProviderRegistration = "provider-registration";
}
