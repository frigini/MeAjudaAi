namespace MeAjudaAi.Modules.Providers.Domain.Enums;

/// <summary>
/// Tipo de prestador de serviços (Individual ou Company).
/// </summary>
public enum EProviderType
{
    /// <summary>
    /// Tipo não definido
    /// </summary>
    None = 0,

    Individual = 1,
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
