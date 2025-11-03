namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Tipo de prestador de serviços: None, Individual, Company, Cooperative, Freelancer.
/// </summary>
public enum EProviderType
{
    /// <summary>
    /// Tipo não definido
    /// </summary>
    None = 0,

    /// <summary>
    /// Individual provider
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Company provider
    /// </summary>
    Company = 2,

    /// <summary>
    /// Cooperativa
    /// </summary>
    Cooperative = 3,

    /// <summary>
    /// Autônomo
    /// </summary>
    Freelancer = 4
}
