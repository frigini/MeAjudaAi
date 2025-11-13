namespace MeAjudaAi.Shared.Application.Interfaces;

/// <summary>
/// Status de uma verificação de antecedentes criminais
/// </summary>
public enum BackgroundCheckStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    NotAvailable
}

/// <summary>
/// Resultado de uma verificação de antecedentes criminais
/// </summary>
public record BackgroundCheckResult(
    string RequestId,
    BackgroundCheckStatus Status,
    bool? HasCriminalRecord,
    string? Details,
    DateTime? CompletedAt,
    string? ErrorMessage);

/// <summary>
/// Interface para serviços de verificação de antecedentes criminais
/// 
/// NOTA: Esta é uma interface de contrato. A implementação concreta dependerá
/// do provedor escolhido (ex: Serasa Experian, APIs do Tribunal de Justiça, etc).
/// 
/// Provedores sugeridos para o Brasil:
/// - Serasa Experian (APIs comerciais)
/// - Certidões online dos Tribunais de Justiça estaduais
/// - CNJ (Conselho Nacional de Justiça) - se disponível via API
/// </summary>
public interface IBackgroundCheckService
{
    /// <summary>
    /// Solicita uma verificação de antecedentes criminais
    /// </summary>
    /// <param name="cpf">CPF da pessoa a ser verificada</param>
    /// <param name="fullName">Nome completo da pessoa</param>
    /// <param name="birthDate">Data de nascimento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado inicial com RequestId para acompanhamento</returns>
    Task<BackgroundCheckResult> RequestCheckAsync(
        string cpf,
        string fullName,
        DateTime birthDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o status de uma verificação previamente solicitada
    /// </summary>
    /// <param name="requestId">ID da requisição retornado por RequestCheckAsync</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status atualizado da verificação</returns>
    Task<BackgroundCheckResult> GetCheckStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default);
}
