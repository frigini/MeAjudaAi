namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de feature flags.
/// Usado com Microsoft.FeatureManagement para controle dinâmico de funcionalidades.
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Feature flag para restrição geográfica (cidades piloto MVP).
    /// Quando habilitado, apenas cidades configuradas em AllowedCities podem acessar.
    /// </summary>
    public const string GeographicRestriction = "GeographicRestriction";
}
