using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de políticas de rate limiting
/// </summary>
[ExcludeFromCodeCoverage]
public static class RateLimitPolicies
{
    /// <summary>
    /// Política para endpoints públicos anonimizados
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Política para registro de clientes
    /// </summary>
    public const string Registration = "registration";

    /// <summary>
    /// Política para registro de prestadores
    /// </summary>
    public const string ProviderRegistration = "provider-registration";
}
