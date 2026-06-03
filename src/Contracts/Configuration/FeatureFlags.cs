namespace MeAjudaAi.Contracts.Configuration;

/// <summary>
/// Feature flags para habilitar/desabilitar funcionalidades no frontend.
/// </summary>
public sealed record FeatureFlags
{
    /// <summary>
    /// Habilita Redux DevTools (somente em desenvolvimento).
    /// </summary>
    public bool EnableReduxDevTools { get; init; }

    /// <summary>
    /// Habilita mode de debug/diagnóstico.
    /// </summary>
    public bool EnableDebugMode { get; init; }

    /// <summary>
    /// Habilita autenticação fake para desenvolvimento local.
    /// </summary>
    public bool EnableFakeAuth { get; init; }
}
