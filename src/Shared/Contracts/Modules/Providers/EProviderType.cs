namespace MeAjudaAi.Shared.Contracts.Modules.Providers;

/// <summary>
/// Tipos de provedores de serviços
/// </summary>
public enum EProviderType
{
    /// <summary>
    /// Valor padrão - nenhum tipo definido
    /// </summary>
    None = 0,

    /// <summary>
    /// Prestador individual
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Empresa
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
