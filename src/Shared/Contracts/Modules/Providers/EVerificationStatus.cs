namespace MeAjudaAi.Shared.Contracts.Modules.Providers;

/// <summary>
/// Status de verificação do provider
/// </summary>
public enum EVerificationStatus
{
    /// <summary>
    /// Pendente de verificação
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Em processo de verificação
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Verificado com sucesso
    /// </summary>
    Verified = 3,

    /// <summary>
    /// Rejeitado na verificação
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Suspenso temporariamente
    /// </summary>
    Suspended = 5
}