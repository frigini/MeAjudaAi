namespace MeAjudaAi.Modules.Location.Domain.Enums;

/// <summary>
/// Provedores de consulta de CEP disponíveis.
/// Uso futuro: permitir configuração da ordem de fallback ou seleção de provedor específico.
/// </summary>
public enum ECepProvider
{
    /// <summary>
    /// ViaCEP - API pública brasileira de CEPs (geralmente mais rápida).
    /// </summary>
    ViaCep,

    /// <summary>
    /// BrasilAPI - API agregadora de dados públicos brasileiros.
    /// </summary>
    BrasilApi,

    /// <summary>
    /// OpenCEP - API pública alternativa de CEPs.
    /// </summary>
    OpenCep
}
