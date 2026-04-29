namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para eventos SSE de verificação de prestador.
/// </summary>
public record ProviderVerificationSseDto(
    Guid ProviderId,
    string Status,
    DateTime UpdatedAt,
    string? RejectionReason = null);
