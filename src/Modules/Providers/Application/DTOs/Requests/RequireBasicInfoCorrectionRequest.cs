using MeAjudaAi.Contracts;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para solicitar correção de informações básicas de um prestador de serviços.
/// </summary>
public record RequireBasicInfoCorrectionRequest
{
    /// <summary>
    /// Motivo detalhado da correção necessária (obrigatório).
    /// </summary>
    /// <remarks>
    /// Este campo será enviado ao prestador para que ele saiba quais informações
    /// precisam ser corrigidas ou complementadas.
    /// </remarks>
    public string Reason { get; init; } = string.Empty;
}
