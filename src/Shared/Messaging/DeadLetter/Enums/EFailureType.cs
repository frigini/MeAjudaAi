namespace MeAjudaAi.Shared.Messaging.DeadLetter.Enums;

/// <summary>
/// Enumeração dos tipos de falha para classificação
/// </summary>
public enum EFailureType
{
    /// <summary>
    /// Tipo não definido
    /// </summary>
    None = 0,

    /// <summary>
    /// Falha temporária (rede, timeout, etc.) - retry recomendado
    /// </summary>
    Transient,

    /// <summary>
    /// Falha permanente (validação, regra de negócio) - não retry
    /// </summary>
    Permanent,

    /// <summary>
    /// Falha crítica do sistema - necessita investigação
    /// </summary>
    Critical,

    /// <summary>
    /// Falha desconhecida - usar configuração padrão
    /// </summary>
    Unknown
}
