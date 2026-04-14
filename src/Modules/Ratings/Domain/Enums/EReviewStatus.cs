namespace MeAjudaAi.Modules.Ratings.Domain.Enums;

/// <summary>
/// Status de uma avaliação de prestador.
/// </summary>
public enum EReviewStatus
{
    /// <summary>
    /// Avaliação aguardando moderação.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Avaliação aprovada e visível publicamente.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Avaliação rejeitada por violação de termos.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Avaliação sinalizada para revisão manual.
    /// </summary>
    Flagged = 3
}
