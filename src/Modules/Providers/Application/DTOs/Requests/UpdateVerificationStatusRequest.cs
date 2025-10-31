using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para atualização do status de verificação de um prestador de serviços.
/// </summary>
public record UpdateVerificationStatusRequest : Request
{
    /// <summary>
    /// Novo status de verificação.
    /// </summary>
    public EVerificationStatus Status { get; init; }

    /// <summary>
    /// Observações sobre a verificação (opcional).
    /// </summary>
    public string? Notes { get; init; }
}